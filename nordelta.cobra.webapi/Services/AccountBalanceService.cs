using nordelta.cobra.webapi.Repositories.Contracts;
using nordelta.cobra.webapi.Services.Contracts;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using nordelta.cobra.webapi.Models;
using nordelta.cobra.webapi.Services.DTOs;
using Serilog;
using Hangfire;
using nordelta.cobra.webapi.Services.Helpers;
using nordelta.cobra.webapi.Helpers;
using RestSharp;
using Microsoft.Extensions.Options;
using nordelta.cobra.webapi.Configuration;
using nordelta.cobra.webapi.Controllers.Helpers;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;
using Microsoft.OpenApi.Extensions;
using System.Runtime.Serialization;
using nordelta.cobra.webapi.Utils;
using nordelta.cobra.webapi.Services.Records;

namespace nordelta.cobra.webapi.Services
{
    public class AccountBalanceService : IAccountBalanceService
    {
        private readonly IAccountBalanceRepository _accountBalanceRepository;
        private readonly IForeignCuitCacheRepository _foreignCuitRepository;
        private readonly IPaymentService _paymentService;
        private readonly IUserRepository _userRepository;
        private readonly IConfiguration _configuration;
        private readonly IUserService _userService;
        private readonly IMailService _mailService;
        private readonly IRestClient _restClient;
        private readonly IOptionsMonitor<ApiServicesConfig> _apiServicesConfig;
        private readonly IOptionsMonitor<TiposOperacionesConfiguration> _tiposOperacionesConfig;

        public AccountBalanceService(
            IAccountBalanceRepository accountBalanceRepository,
            IForeignCuitCacheRepository foreignCuitRepository,
            IPaymentService paymentService,
            IUserRepository userRepository,
            IConfiguration configuration,
            IUserService userService,
            IMailService mailService,
            IRestClient restClient,
            IOptionsMonitor<ApiServicesConfig> apiServicesConfig,
            IOptionsMonitor<TiposOperacionesConfiguration> tiposOperacionesConfiguration
            )
        {
            _accountBalanceRepository = accountBalanceRepository;
            _foreignCuitRepository = foreignCuitRepository;
            _paymentService = paymentService;
            _userRepository = userRepository;
            _configuration = configuration;
            _userService = userService;
            _mailService = mailService;
            _restClient = restClient;
            _restClient.Timeout = 10 * 60000; // Milliseconds
            _restClient.ThrowOnAnyError = true;
            _apiServicesConfig = apiServicesConfig;
            _tiposOperacionesConfig = tiposOperacionesConfiguration;
        }

        private async Task<List<ApplicationDetailSummaryDto>> GetApplicationDetailSummaryAsync(IEnumerable<string> cuits, bool foreignCuits = false)
        {
            try
            {
                var requestModel = new ResumenCuentaRequest
                {
                    NroDocumentos = cuits.ToList()
                };

                var applicationDetails = await _paymentService.GetApplicationDetailAsync(requestModel);

                var tiposOperaciones = _tiposOperacionesConfig.Get(TiposOperacionesConfiguration.Aplicaciones).Operaciones;
                Log.Debug("Tipo de operaciones Aplicaciones: {tipos}", string.Join(", ", tiposOperaciones));
                var filteredResults = applicationDetails.Where(x => tiposOperaciones.Any(y => x.OperationType.ToUpper() == y)).ToList();

                // Tomo las columnas que necesito
                var filteredConvertedResults = filteredResults
                    .Select(x => new
                    {
                        x.CustomerDocumentNumber,
                        x.Producto,
                        x.ReferenciaCliente,
                        RazonSocial = x.AccountName,
                        x.InvoiceCurrencyCode,
                        AmountApplied = Convert.ToDecimal(x.ApplRcvUsd, new CultureInfo("en-US")),
                        Amount = Convert.ToDecimal(x.Amount, new CultureInfo("en-US")),
                        x.TrxNumber,
                        ApplTc = Convert.ToDecimal(x.ApplTc, new CultureInfo("en-US"))
                    }).ToList();

                // Agrupo por Cuit | CodigoProducto | ReferenciaCliente | NumeroTransaction
                var result = filteredConvertedResults
                    .GroupBy(l => new { l.CustomerDocumentNumber, l.Producto, l.TrxNumber })
                    .Select(cl => new ApplicationDetailSummaryDto()
                    {
                        Cuit = cl.Key.CustomerDocumentNumber,
                        CurrencyCode = Currency.ARS.GetAttributeOfType<EnumMemberAttribute>().Value,
                        Product = cl.Key.Producto,
                        Quantity = cl.Count(),
                        TotalAmountApplied = cl.Sum(c => c.AmountApplied),
                        ClientReference = cl.First().ReferenciaCliente,
                        RazonSocial = cl.First().RazonSocial
                    }).ToList();
                            
                var applicationDetailSumary = new List<ApplicationDetailSummaryDto>();
                if (foreignCuits)
                {
                    applicationDetailSumary = result
                         .GroupBy(l => new { l.Product, l.Cuit, l.ClientReference })
                         .Select(x => new ApplicationDetailSummaryDto
                         {
                             Cuit = x.Key.Cuit,
                             Product = x.Key.Product,
                             ClientReference = x.Key.ClientReference,
                             RazonSocial = x.First().RazonSocial,
                             CurrencyCode = x.First().CurrencyCode,
                             Quantity = x.Count(),
                             TotalAmountApplied = x.Sum(c => c.TotalAmountApplied)
                         }).ToList();
                }
                else
                {
                    applicationDetailSumary = result
                        .GroupBy(l => new { l.Product, l.Cuit })
                        .Select(x => new ApplicationDetailSummaryDto
                        {
                            Cuit = x.Key.Cuit,
                            Product = x.Key.Product,
                            RazonSocial = x.First().RazonSocial,
                            CurrencyCode = x.First().CurrencyCode,
                            Quantity = x.Count(),
                            TotalAmountApplied = x.Sum(c => c.TotalAmountApplied)
                        }).ToList();
                }

                return applicationDetailSumary;
            }
            catch (Exception ex)
            {
                Log.Error(ex, "Error GetApplicationDetailSummaryAsync, msg: {msg}", ex.Message);
                throw;
            }
        }

        private async Task<List<BalanceDetailSummaryDto>> GetBalanceDetailSummaryAsync(IEnumerable<string> cuits, bool foreignCuits = false)
        {
            try
            {
                var requestModel = new ResumenCuentaRequest
                {
                    NroDocumentos = cuits.ToList()
                };

                var balanceDetails = await _paymentService.GetBalanceDetailAsync(requestModel);
                var result = new List<BalanceDetailSummaryDto>();
                var nowDate = LocalDateTime.GetDateTimeNow();

                var tiposOperaciones = _tiposOperacionesConfig.Get(TiposOperacionesConfiguration.Saldos).Operaciones;
                Log.Debug("Tipo de operaciones Saldos: {tipos}", string.Join(", ", tiposOperaciones));
                var filteredResults = balanceDetails.Where(x => tiposOperaciones.Any(y => x.OperationType.ToUpper() == y)).ToList();

                if (foreignCuits)
                {
                    result = filteredResults
                        .GroupBy(l => new { l.CustomerDocumentNumber, l.Producto, l.ReferenciaCliente, l.CurrencyCode })
                        .Select(cl =>
                        {
                            var dueDate = cl.Where(x =>
                                        DateTime.Compare(DateTime.ParseExact(x.DueDate, "dd/MM/yyyy", CultureInfo.InvariantCulture), nowDate) >= 0)
                                        .OrderBy(y => DateTime.ParseExact(
                                            y.DueDate, "dd/MM/yyyy", CultureInfo.InvariantCulture))
                                        .FirstOrDefault()?.DueDate;

                            var balanceDetailSummary = new BalanceDetailSummaryDto()
                            {
                                Cuit = cl.Key.CustomerDocumentNumber,
                                Product = cl.Key.Producto,
                                CurrencyCode = cl.Key.CurrencyCode,
                                ClientReference = cl.Key.ReferenciaCliente,
                                Quantity = cl.Count(),
                                TotalAmountDueRemaining = cl.Sum(c => Convert.ToDecimal(c.AmountDueRemaining, new CultureInfo("en-US"))),
                                OverdueQuantity = cl.Count(x =>
                                    DateTime.Compare(DateTime.ParseExact(x.DueDate, "dd/MM/yyyy", CultureInfo.InvariantCulture), nowDate) < 0),
                                TotalAmountOverdue = cl.Where(x =>
                                    DateTime.Compare(DateTime.ParseExact(x.DueDate, "dd/MM/yyyy", CultureInfo.InvariantCulture), nowDate) < 0).Sum(x => Convert.ToDecimal(x.AmountDueRemaining, new CultureInfo("en-US"))),
                                FutureQuantity = cl.Count(x =>
                                    DateTime.Compare(DateTime.ParseExact(x.DueDate, "dd/MM/yyyy", CultureInfo.InvariantCulture), nowDate) >= 0),
                                TotalAmountFuture = cl.Where(x =>
                                    DateTime.Compare(DateTime.ParseExact(x.DueDate, "dd/MM/yyyy", CultureInfo.InvariantCulture), nowDate) >= 0).Sum(x => Convert.ToDecimal(x.AmountDueRemaining, new CultureInfo("en-US"))),
                                OverdueDate = dueDate,
                                PublishDebt = cl.First().PublishDebt,
                                RazonSocial = cl.First().AccountName
                            };
                            return balanceDetailSummary;
                        }).ToList();
                }
                else
                {
                    result = filteredResults
                        .GroupBy(l => new { l.CustomerDocumentNumber, l.Producto, l.CurrencyCode })
                        .Select(cl =>
                        {
                            var dueDate = cl.Where(x =>
                                        DateTime.Compare(DateTime.ParseExact(x.DueDate, "dd/MM/yyyy", CultureInfo.InvariantCulture), nowDate) >= 0)
                                        .OrderBy(y => DateTime.ParseExact(
                                            y.DueDate, "dd/MM/yyyy", CultureInfo.InvariantCulture))
                                        .FirstOrDefault()?.DueDate;

                            var balanceDetailSummary = new BalanceDetailSummaryDto()
                            {
                                Cuit = cl.Key.CustomerDocumentNumber,
                                Product = cl.Key.Producto,
                                CurrencyCode = cl.Key.CurrencyCode,
                                //ClientReference = cl.First().ReferenciaCliente,
                                Quantity = cl.Count(),
                                TotalAmountDueRemaining = cl.Sum(c => Convert.ToDecimal(c.AmountDueRemaining, new CultureInfo("en-US"))),
                                OverdueQuantity = cl.Count(x =>
                                    DateTime.Compare(DateTime.ParseExact(x.DueDate, "dd/MM/yyyy", CultureInfo.InvariantCulture), nowDate) < 0),
                                TotalAmountOverdue = cl.Where(x =>
                                    DateTime.Compare(DateTime.ParseExact(x.DueDate, "dd/MM/yyyy", CultureInfo.InvariantCulture), nowDate) < 0).Sum(x => Convert.ToDecimal(x.AmountDueRemaining, new CultureInfo("en-US"))),
                                FutureQuantity = cl.Count(x =>
                                    DateTime.Compare(DateTime.ParseExact(x.DueDate, "dd/MM/yyyy", CultureInfo.InvariantCulture), nowDate) >= 0),
                                TotalAmountFuture = cl.Where(x =>
                                    DateTime.Compare(DateTime.ParseExact(x.DueDate, "dd/MM/yyyy", CultureInfo.InvariantCulture), nowDate) >= 0).Sum(x => Convert.ToDecimal(x.AmountDueRemaining, new CultureInfo("en-US"))),
                                OverdueDate = dueDate,
                                PublishDebt = cl.First().PublishDebt,
                                RazonSocial = cl.First().AccountName
                            };
                            return balanceDetailSummary;
                        }).ToList();
                }

                return result;
            }
            catch (Exception ex)
            {
                Log.Error(ex, "Error GetBalanceDetailSummaryAsync, msg: {msg}", ex.Message);
                throw;
            }
        }

        private List<CuitLotDto> GetCuitLots(IEnumerable<string> userCuits)
        {
            var foreignCuits = _foreignCuitRepository.GetAll();
            var userCuitsExtranjeros = new CuitLotDto
            {
                Cuits = userCuits.Where(x => foreignCuits.Any(y => y.Cuit.Equals(x))).ToList(),
                AreForeigners = true
            };

            var userCuitsNoExtranjeros = new CuitLotDto
            {
                Cuits = userCuits.Where(x => !foreignCuits.Any(y => y.Cuit.Equals(x))).ToList(),
                AreForeigners = false
            };

            var cuitLots = new List<CuitLotDto> { userCuitsExtranjeros, userCuitsNoExtranjeros };

            return cuitLots;
        }

        private async Task<List<ApplicationDetailSummaryDto>> GetApplicationDetailSummaryByCuitsAsync(IEnumerable<string> userCuits)
        {
            var result = new List<ApplicationDetailSummaryDto>();
            var cuitLots = GetCuitLots(userCuits);
            
            foreach (var cuitLot in cuitLots)
            {
                var cuitsLotesToProcess = cuitLot.Cuits
                    .Select((cuit, index) => new { cuit, index })
                    .GroupBy(x => x.index / 1000) // <= Limite de cantidad de cuits por request a SGF
                    .Select(g => g.Select(y => y.cuit).ToList())
                    .ToList();
                
                foreach (var cuitsLoteToProcess in cuitsLotesToProcess)
                {
                    var applicationDetailSummary = await GetApplicationDetailSummaryAsync(cuitsLoteToProcess, cuitLot.AreForeigners);

                    if (!applicationDetailSummary.Any())
                    {
                        throw new Exception("No se encontrarón applicationDetails de SGF");
                    }

                    result.AddRange(applicationDetailSummary);
                }
            }

            return result;
        }

        private async Task<List<BalanceDetailSummaryDto>> GetBalanceDetailSummaryByCuitsAsync(IEnumerable<string> userCuits)
        {
            var result = new List<BalanceDetailSummaryDto>();
            var cuitLots = GetCuitLots(userCuits);

            foreach (var cuitLot in cuitLots)
            {
                var cuitsLotesToProcess = cuitLot.Cuits
                    .Select((cuit, index) => new { cuit, index })
                    .GroupBy(x => x.index / 1000) // <= Limite de cantidad de cuits por request a SGF
                    .Select(g => g.Select(y => y.cuit).ToList())
                    .ToList();

                foreach (var cuitsLoteToProcess in cuitsLotesToProcess)
                {
                    var balanceDetailSummary = await GetBalanceDetailSummaryAsync(cuitsLoteToProcess, cuitLot.AreForeigners);

                    if (!balanceDetailSummary.Any())
                    {
                        throw new Exception("No se encontrarón balanceDetails de SGF");
                    }

                    result.AddRange(balanceDetailSummary);
                }
            }

            return result;
        }

        [Queue("default")]
        [DisableConcurrentExecution(timeoutInSeconds: 1800)]
        public async Task UpdateAccountBalancesAsync()
        {
            try
            {
                Log.Information("Start UpdateAccountBalancesAsync...");

                // GET USERCUITS
                var userCuits = _userRepository.GetAllUsers()
                    .SelectMany(x => x.UserDataCuits)
                    .GroupBy(x => x.Cuit)
                    .Select(x => new UserCuitDto()
                    {
                        UserId = x.First().UserId,
                        Cuit = x.First().Cuit
                    }).ToList();
                Log.Debug("Usuarios obtenidos de SSO: {ssoUserCount}", userCuits.Count);

                // GET BALANCES 
                var applications = await GetApplicationDetailSummaryByCuitsAsync(userCuits.Select(x => x.Cuit));
                Log.Debug("Aplicaciones obtenidas de SGF: {applicationCount}", applications.Count);

                // GET APPLICATIONS
                var balances = await GetBalanceDetailSummaryByCuitsAsync(userCuits.Select(x => x.Cuit));
                Log.Debug("Balances obtenidos de SGF: {balanceCount}", balances.Count);

                // GET BALANCE APPLICATION
                var productCodeCuits = applications.Select(x => new ProductCodeCuitRecord(x)).ToList();
                productCodeCuits.AddRange(balances.Select(x => new ProductCodeCuitRecord(x)).ToList());
                productCodeCuits = productCodeCuits.Distinct().ToList();

                var balanceApplications = new List<BalanceApplicationDto>();

                foreach (var productCodeCuit in productCodeCuits)
                {
                    var application = applications.FirstOrDefault(x => x.Product.Equals(productCodeCuit.Codigo) && 
                                                                       x.Cuit.Equals(productCodeCuit.Cuit) &&
                                                                       // En caso de que sea un cliente extranjero se toma en cuenta clientReference
                                                                       (x.ClientReference == null || x.ClientReference.Equals(productCodeCuit.ClientReference)));

                    var balance = balances.FirstOrDefault(x => x.Product.Equals(productCodeCuit.Codigo) && 
                                                               x.Cuit.Equals(productCodeCuit.Cuit) &&
                                                               // En caso de que sea un cliente extranjero se toma en cuenta clientReference
                                                               (x.ClientReference == null || x.ClientReference.Equals(productCodeCuit.ClientReference)));

                    var balanceApplication = new BalanceApplicationDto()
                    {
                        Cuit = productCodeCuit.Cuit,
                        Product = productCodeCuit.Codigo,
                        ClientReference = productCodeCuit.ClientReference,

                        RazonSocial = application?.RazonSocial ?? balance.RazonSocial,
                        CurrencyCode = application?.CurrencyCode ?? balance.CurrencyCode,
                        QuantityApplied = application?.Quantity ?? 0,
                        TotalAmountApplied = application?.TotalAmountApplied ?? 0,
                        QuantityRemaining = balance?.Quantity ?? 0,
                        TotalAmountDueRemaining = balance?.TotalAmountDueRemaining ?? 0,
                        OverdueQuantity = balance?.OverdueQuantity ?? 0,
                        OverdueDate = balance?.OverdueDate ?? string.Empty,
                        TotalAmountOverdue = balance?.TotalAmountOverdue ?? 0,
                        FutureQuantity = balance?.FutureQuantity ?? 0,
                        TotalAmountFuture = balance?.TotalAmountFuture ?? 0,
                        PublishDebt = balance?.PublishDebt
                    };

                    balanceApplications.Add(balanceApplication);
                }

                // GET SALES INVOICES
                var porductCuitsTest = productCodeCuits.Select(x => new PropertyCodeCuitDto { Cuit = x.Cuit, Codigo = x.Codigo }).ToList();
                var salesInvoices = _paymentService.GetSalesInvoiceAmount(porductCuitsTest);
                Log.Debug("SalesInvoice obtenidos de SGC: {saleInvoiceCount}", salesInvoices.Count);

                // GET ACCOUNT BALANCES
                var accountBalances = (
                    from balanceApplication in balanceApplications
                    join userCuit in userCuits on balanceApplication.Cuit equals userCuit.Cuit
                    join sale in salesInvoices on new { Codigo = balanceApplication.Product, balanceApplication.Cuit } equals new { sale.Codigo, sale.Cuit } into userSale
                    from saleJoin in userSale.DefaultIfEmpty()
                    select new AccountBalance()
                    {
                        Product = balanceApplication.Product,
                        ClientId = userCuit.UserId,
                        ClientCuit = balanceApplication.Cuit,
                        RazonSocial = balanceApplication.RazonSocial,
                        ClientReference = balanceApplication.ClientReference, // TODO: Si "ClientReference" no es nulo entonces es "CUIT EXTRANJERO" caso contrario es "CUIT NO EXTRANJERO"
                        Balance = balanceApplication.OverdueQuantity == 0 ? AccountBalance.EBalance.AlDia : AccountBalance.EBalance.Mora,
                        ContactStatus = AccountBalance.EContactStatus.NoContactado,
                        Department = AccountBalance.EDepartment.CuentasACobrar,

                        FuturePaymentsAmountUSD = decimal.ToDouble(balanceApplication.TotalAmountFuture),
                        FuturePaymentsCount = balanceApplication.FutureQuantity,

                        OverduePaymentDate = balanceApplication.OverdueDate,
                        OverduePaymentsAmountUSD = decimal.ToDouble(balanceApplication.TotalAmountOverdue),
                        OverduePaymentsCount = balanceApplication.OverdueQuantity,

                        PaidPaymentsAmountUSD = decimal.ToDouble(balanceApplication.TotalAmountApplied),
                        PaidPaymentsCount = balanceApplication.QuantityApplied,

                        TotalDebtAmount = decimal.ToDouble(balanceApplication.TotalAmountDueRemaining),
                        SalesInvoiceAmountUSD = balanceApplication.ClientReference is not null ? 0 : // TODO: Para cuit extrajero setear 0 hasta nueva definicion
                                                saleJoin != null ? decimal.ToDouble(saleJoin.Monto) : 0,

                        PublishDebt = balanceApplication.PublishDebt
                    }).ToList();

                Log.Information("AccountBalances creados: {accountBalanceCount}", accountBalances.Count);

                // ADD BUSSINES UNIT
                var productCodes = accountBalances.Select(x => x.Product).ToList();
                var buList = _paymentService.GetBusinessUnitByProductCodes(productCodes);
                if (buList.Count > 0)
                {
                    accountBalances.ForEach(x =>
                    {
                        x.BusinessUnit = buList.FirstOrDefault(y => y.Codigo == x.Product)?.BusinessUnit;
                    });
                }

                // ADD OR UPDATE ACCOUNT BALANCES
                var ret = _accountBalanceRepository.InsertOrUpdate(accountBalances);
                Log.Information("Finalizo UpdateAccountBalancesAsync se actualizaron: {@r}", ret.Count);
            }
            catch (Exception ex)
            {
                Log.Error(ex, "Error al sincronizar balances de cuenta. Exception Detail: {msg}", ex.Message);
            }
        }
        
        public async Task SyncRazonesSocialesAsync()
        {
            Log.Information("Start SyncRazonesSocialesAsync...");

            try
            {
                var accountBalances = _accountBalanceRepository.GetAll(x => x.RazonSocial == null);
                var users = _userRepository.GetAllUsers();
                var userCuits = _userRepository.GetAllUsers().SelectMany(x => x.UserDataCuits).ToList();
                var updatedAccountBalances = new List<AccountBalance>();

                foreach (var accountBalance in accountBalances)
                {
                    var razonSocial = users.FirstOrDefault(x => x.Cuit == accountBalance.ClientCuit)?.RazonSocial ??
                                      userCuits.FirstOrDefault(x => x.Cuit == accountBalance.ClientCuit)?.RazonSocial;

                    if (razonSocial is not null)
                    {
                        accountBalance.RazonSocial = razonSocial;
                        updatedAccountBalances.Add(accountBalance);
                    }
                }

                Log.Information("SyncRazonesSocialesAsync(): Se obtuvieron {count} Account Balances para actualizar", updatedAccountBalances.Count);
                await _accountBalanceRepository.UpdateAll(updatedAccountBalances);
            }
            catch (Exception ex)
            {
                Log.Error(ex, "SyncRazonesSocialesAsync(): Error al actualizar razones sociales de account balance. Exception Detail: {msg}", ex.Message);
                throw;
            }
        }

        public bool UpdateAccountBalance(AccountBalanceDTO accountBalanceDto, User user)
        {
            var accountBalanceInDb = _accountBalanceRepository.GetAccountBalanceById(accountBalanceDto.Id);
            if (accountBalanceInDb != null)
            {
                _accountBalanceRepository.AddToLogAccountBalance(accountBalanceDto, accountBalanceInDb, user);
            }
            if (accountBalanceDto.Department != accountBalanceInDb.Department)
            {
                accountBalanceInDb.Department = accountBalanceDto.Department;
            }

            if (accountBalanceDto.DelayStatus != accountBalanceInDb.DelayStatus)
            {
                accountBalanceInDb.DelayStatus = accountBalanceDto.DelayStatus;

            }

            if (accountBalanceDto.WorkStarted != accountBalanceInDb.WorkStarted)
            {
                accountBalanceInDb.WorkStarted = accountBalanceDto.WorkStarted;
            }

            if (!string.IsNullOrEmpty(accountBalanceDto.PublishDebt) && accountBalanceDto.PublishDebt != accountBalanceInDb.PublishDebt)
            {
                try
                {
                    accountBalanceInDb.PublishDebt = _paymentService.UpdatePublishDebt(accountBalanceInDb.ClientCuit, accountBalanceInDb.Product, accountBalanceDto.PublishDebt);
                }
                catch (Exception ex)
                {
                    Log.Error(ex, "Error actualizando la publicacion de Deuda. cuit: {clientCuit}, producto: {product}, publishDebt: {publishDebt}", accountBalanceInDb.ClientCuit, accountBalanceInDb.Product, accountBalanceDto.PublishDebt);
                }
            }

            var result = _accountBalanceRepository.InsertOrUpdate(accountBalanceInDb);

            if (result)
            {
                _accountBalanceRepository.CheckLegales(accountBalanceDto.Department, accountBalanceDto.Id);
                return result;
            }

            return result;
        }

        public List<DeudaMoraResponseDto> GetAllDeudaMoraByProduct(List<DeudaMoraRequestDto> model)
        {
            var result = new List<DeudaMoraResponseDto>();
            try
            {
                foreach (var m in model)
                {
                    foreach (var cuit in m.Cuits)
                    {
                        var accountBal = _accountBalanceRepository.GetAccountBalanceByProductCuit(m.CodProducto, cuit);
                        if (accountBal != null)
                        {
                            var resp = new DeudaMoraResponseDto()
                            {
                                IdOperacionProducto = m.IdOperacionProducto,
                                CodProducto = accountBal.Product,
                                Cuit = accountBal.ClientCuit,
                                Deuda = accountBal.TotalDebtAmount > 0,
                                Mora = accountBal.OverduePaymentsCount > 0
                            };
                            result.Add(resp);
                        }
                    }
                }

                return result;
            }
            catch (Exception ex)
            {
                Log.Error("Error al obtener Deuda y Mora de Account Balance: {@ex}, model: {@model}", ex, model);
                throw new Exception("Error al obtener Deuda y Mora de Account Balance. Exception Detail: " + ex.Message);
            }
        }

        public List<ClientProductsDto> GetClientProductsByCuits(List<string> cuits)
        {
            var clientProducts = new List<ClientProductsDto>();
            var accounts = _accountBalanceRepository.GetAccountBalanceByCuits(cuits);
            var productCodes = accounts.Select(x => x.Product).Distinct().ToList();
            _restClient.BaseUrl = new Uri(_apiServicesConfig.Get(ApiServicesConfig.SgcApi).Url);
            RestRequest request = new RestRequest("/Producto/Emprendimientos", Method.POST)
            {
                RequestFormat = DataFormat.Json
            };


            request.AddHeader("token", _apiServicesConfig.Get(ApiServicesConfig.SgcApi).Token);
            request.AddJsonBody(productCodes);
            try
            {
                IRestResponse<List<EmprendimientosResponse>> response = AsyncHelper.RunSync(
                    async () => await _restClient.ExecuteAsync<List<EmprendimientosResponse>>(request));

                if (response.IsSuccessful && response.Data != null)
                {
                    Log.Debug(@"GetAccountBalancesByCuits, 
                        Type: GetAccountBalancesByCuitsSuccess,
                        Description: Response successful for PropertyCodes:
                        request: {@request},
                        response: {@response}", request, response);
                    clientProducts = accounts.Select(x => new ClientProductsDto
                    {
                        BU = x.BusinessUnit,
                        MonedaSaldoPendiente = Currency.USD,
                        SaldoPendiente = Math.Round(x.TotalDebtAmount, 2),
                        MonedaTotalPagado = Currency.USD,
                        TotalPagado = Math.Round(x.PaidPaymentsAmountUSD, 2),
                        Product = x.Product.Trim()
                    }).ToList();
                    foreach (var clientProductsDto in clientProducts)
                    {
                        try
                        {
                            clientProductsDto.MontoTotal = response
                                .Data
                                .SingleOrDefault(y => y.codigo.Trim().Equals(clientProductsDto.Product))
                                ?.precioPactadoValor;
                            clientProductsDto.Emprendimiento = response
                                .Data
                                .SingleOrDefault(y => y.codigo.Trim().Equals(clientProductsDto.Product))
                                ?.emprendimiento;
                        }
                        catch (Exception ex)
                        {
                            Log.Warning(ex, @"GetAccountBalancesByCuits,
                                Type: GetAccountBalancesByCuitsWarning,
                                Description: MoreThanOneMatchException in GetEmprendimiento for
                            product: { prod }
                            ", clientProductsDto.Product);
                        }
                    }
                    return clientProducts;
                }
                else    
                {
                    Log.Information(@"GetAccountBalancesByCuits, 
                        Type:GetAccountBalancesByCuitsWarning,
                        Description: Response not successful or Data is null:
                        request: {@request},
                        response: {@response}", request, response);
                    return null;
                }
            }
            catch (Exception e)
            {
                Log.Error(@"GetAccountBalancesByCuits, 
                    Type:GetAccountBalancesByCuitsError,
                    Description: Error fetching Emprendimientos for products:
                    request: {@request},
                    response: {@error}", request, e);
                throw new Exception("Error al obtener los emprendimientos del Sgc: " + e.Message);
            }
        }

        public List<AccountBalance> GetAllAccountBalances(User user, string search, string project, int? department, int? balance)
        {
            var accounts = new List<AccountBalance>();
            try
            {
                accounts = _accountBalanceRepository.GetAllAccountBalances(user, search, project, department, balance);

                foreach (var accountBalance in accounts)
                {
                    accountBalance.Communications?.ForEach(x => x.SsoUser = _userRepository.GetSsoUserById(x.CommunicationCreatorUserId));
                    accountBalance.Client = new User
                    {
                        FirstName = !string.IsNullOrEmpty(accountBalance.RazonSocial) ? accountBalance.RazonSocial : "Sin Razon Social"
                    };
                }

                return accounts;
            }
            catch (Exception ex)
            {
                Log.Error("No se pudo obtener los balances de cuenta. Exception Detail: {@ex}", ex);
                return accounts;
            }
        }

        public AccountBalancePagination GetAllAccountBalances(User user, int pageSize, int pageNumber, string search, string project, int? department, int? balance)
        {
            var accounts = new AccountBalancePagination();
            try
            {
                var result = _accountBalanceRepository.GetAllAccountBalances(user, search, project, department, balance);

                accounts.AccountBalances = result.Skip(pageSize * (pageNumber - 1)).Take(pageSize).ToList();
                accounts.TotalCount = result.Count;

                foreach (var accountBalance in accounts.AccountBalances)
                {
                    accountBalance.Communications?.ForEach(x => x.SsoUser = _userRepository.GetSsoUserById(x.CommunicationCreatorUserId));
                    accountBalance.Client = new User
                    {
                        FirstName = !string.IsNullOrEmpty(accountBalance.RazonSocial) ? accountBalance.RazonSocial : "Sin Razon Social"
                    };
                }

                return accounts;
            }
            catch (Exception ex)
            {
                Log.Error("No se pudo obtener los balances de cuenta. Exception Detail: {@ex}", ex);
                return accounts;
            }
        }

        public List<AccountBalanceDetailDto> GetAccountBalanceDetail(AccountBalance accountBalance)
        {
            try
            {
                var balanceDetail = _paymentService.GetRawBalanceDetail(accountBalance.ClientCuit, accountBalance.Product);

                var result = balanceDetail.Select(x => new AccountBalanceDetailDto()
                {
                    AccountBalanceId = accountBalance.Id,
                    CodigoMoneda = x.CurrencyCode,
                    FechaPrimerVencimiento = DateTime.ParseExact(x.DueDate, "dd/MM/yyyy", CultureInfo.InvariantCulture),
                    ImportePrimerVenc = x.AmountLineItemsRemaining.GetDecimal(), // Capital
                    SaldoActual = x.AmountDueRemaining.GetDecimal(), // Saldo
                    Intereses = x.AmountAdjusted.GetDecimal() // Intereses
                }).ToList();

                return result;
            }
            catch (Exception ex)
            {
                Log.Error("No se pudo obtener el detalle de balance de cuenta: {accountBalanceId}. Exception Detail: {@ex}", accountBalance.Id, ex);
                throw;
            }
        }

        [Queue("mssql")]
        public async Task SendRepeatedLegalEmail()
        {
            if (_configuration.GetSection("ServiceConfiguration:SendRepeatedLegalEmail").Get<bool>())
            {
                var legalesDetails = await _accountBalanceRepository.GetAllLegalesNotificationAsync();
                var filteredLegalesDetails = legalesDetails.Distinct(new DistinctDepartmentChangeNotificationComparer()).OrderBy(x => x.CodigoProducto).ToList();


                var emails = _userService.GetEmailsForRole("Legales").ToList(); //Rol from DB
                if (filteredLegalesDetails.Any() && emails.Any())
                {
                    await this._mailService.SendRepeatedLegalEmail(filteredLegalesDetails, emails);
                    _accountBalanceRepository.CleanLegalesNotification();
                }
            }
        }

        public IEnumerable<string> GetAllAccountBalanceBU(bool isExternal)
        {
            return _accountBalanceRepository.GetAllBU(isExternal);
        }

        public List<AccountBalance> GetClientByProduct(string product)
        {
            return _accountBalanceRepository.GetAccountBalanceByProduct(product);
        }

        public AccountBalance GetAccountBalanceByCuitAndProduct(string cuit, string codProducto)
        {
            return _accountBalanceRepository.GetAccountBalance(cuit, codProducto);
        }

    }
}
