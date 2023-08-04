using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using nordelta.cobra.webapi.Services.Contracts;
using DebitoInmediatoServiceItau;
using nordelta.cobra.webapi.Services.DTOs;
using nordelta.cobra.webapi.Models;
using nordelta.cobra.webapi.Repositories.Contracts;
using nordelta.cobra.webapi.Controllers.ViewModels;
using Hangfire;
using Hangfire.Server;
using Microsoft.Extensions.Configuration;
using nordelta.cobra.webapi.Controllers.Helpers;
using nordelta.cobra.webapi.Models.ArchivoDeuda;
using Serilog;
using nordelta.cobra.webapi.Utils;
using AutoMapper;

namespace nordelta.cobra.webapi.Services
{
    public class DebinService : IDebinService
    {
        private readonly IPaymentService _paymentService;
        private readonly IDebinRepository _debinRepository;
        private readonly ICompanyRepository _companyRepository;
        private readonly IArchivoDeudaRepository _archivoDeudaRepository;
        private readonly IBankAccountRepository _bankAccountRepository;
        private readonly IConfiguration _configuration;
        private readonly IAnonymousPaymentRepository _anonymousPaymentRepository;
        private readonly IItauService _itauService;
        private readonly INotificationService _notificationService;
        private readonly IBackgroundJobClient _backgroundJobClient;
        private readonly IMapper _mapper;

        public DebinService(IPaymentService paymentService,
            IDebinRepository debinRepository,
            ICompanyRepository companyRepository,
            IBankAccountRepository bankAccountRepository,
            IArchivoDeudaRepository archivoDeudaRepository,
            IConfiguration configuration,
            IAnonymousPaymentRepository anonymousPaymentRepository,
            IItauService itauService,
            INotificationService notificationService,
            IBackgroundJobClient backgroundJobClient,
            IMapper mapper
        ) {
            this._debinRepository = debinRepository;
            this._companyRepository = companyRepository;
            this._paymentService = paymentService;
            this._bankAccountRepository = bankAccountRepository;
            this._archivoDeudaRepository = archivoDeudaRepository;
            this._anonymousPaymentRepository = anonymousPaymentRepository;
            this._itauService = itauService;
            this._notificationService = notificationService;
            this._backgroundJobClient = backgroundJobClient;
            _mapper = mapper;
            _configuration = configuration;
        }


        public Task PublishDebin(PublishDebinViewModel publishDebinViewModel, User user, int debinExpirationTimeInMinutes = 0)
        {
            try
            {
                Company currentCompany = _companyRepository.GetByCuit(publishDebinViewModel.VendedorCuit);

                if (currentCompany == null)
                {
                    Log.Warning("PublishDebin: No se encontraron los datos del vendedor {company}",
                        publishDebinViewModel.VendedorCuit);
                    throw new Exception("No se encontraron los datos del Vendedor con cuit " +
                                        publishDebinViewModel.VendedorCuit);
                }

                DateTime now = LocalDateTime.GetDateTimeNow();
                int timeToAddInMinutesDefault = Convert.ToInt32(_configuration
                    .GetSection("DebinDueTimeInMinutes").Value);

                DateTime dueDate = debinExpirationTimeInMinutes > 0
                    ? now.AddMinutes(debinExpirationTimeInMinutes)
                    : now.AddMinutes(timeToAddInMinutesDefault);

                BankAccount currentBankAccount = _bankAccountRepository.GetByCbu(publishDebinViewModel.CompradorCbu);

                if (currentBankAccount == null)
                {
                    Log.Warning("PublishDebin: No se encontraron los datos bancarios del comprador con cuit {cuit}",
                        publishDebinViewModel.CompradorCbu);
                    throw new Exception("No se encontraron los datos bancarios del comprador con cuit" +
                                        publishDebinViewModel.CompradorCbu);
                }

                List<DetalleDeuda> debts = new List<DetalleDeuda>();
                publishDebinViewModel.DebtAmounts.ForEach(debt =>
                {
                    DetalleDeuda auxDebt = _archivoDeudaRepository.Find(debt.DebtId);

                    DetalleDeuda auxDebtDifferentCurrency =
                        _archivoDeudaRepository.FindSameDebtFromAnotherCurrency(auxDebt);

                    debts.Add(auxDebt);

                    if (auxDebtDifferentCurrency != null)
                        debts.Add(auxDebtDifferentCurrency);
                });
                

                var groupedDebtsInDb = debts.GroupBy(d => d.ObsLibreCuarta.Trim()).Select(d => new { codProducto = d.Key, data = d.ToList() });

                foreach (var group in groupedDebtsInDb)
                {
                    var compradorName = user.FirstName;
                    if(compradorName?.Length > 40)
                    {
                        var compradorNameList = user.FirstName.Split(' ');
                        compradorName = compradorNameList.Length > 3 ? 
                            $"{compradorNameList[0]} {compradorNameList[1]} {compradorNameList.Last()}" : 
                            $"{compradorNameList.First()} {compradorNameList.Last()}";
                    }

                    var debtIds = group.data.Where(x => Convert.ToInt32(x.CodigoMoneda.Trim()) == (int)publishDebinViewModel.Moneda).Select(x => x.Id).ToList();

                    var debtsToPay = publishDebinViewModel.DebtAmounts.Where(x => debtIds.Contains(x.DebtId)).ToList();

                    var totalAmount = debtsToPay.Sum(x => x.Amount);

                    RegisterPublicationDTO debinData = new RegisterPublicationDTO()
                    {
                        Vendedor = currentCompany,
                        CompradorCuit = currentBankAccount.Cuit,
                        CompradorCbu = publishDebinViewModel.CompradorCbu,
                        CompradorNombre = compradorName,
                        Producto = group.data.First().NroComprobante,
                        Importe = totalAmount,
                        Moneda = publishDebinViewModel.Moneda,
                        DueDate = dueDate,
                        Now = now,
                        ExternalCode = Guid.NewGuid()
                };

                    _backgroundJobClient.Enqueue(() => PublishDebinTask(debinData, currentCompany, currentBankAccount.Id, user, group.data.ToList()));
                }

                return Task.CompletedTask;
            }
            catch (Exception ex)
            {
                Log.Error(ex, "PublishDebinException debinVM:{@debinVM} user:{@user}", publishDebinViewModel, user);
                throw;
            }
        }

        [Queue("mssql")]
        [JobDisplayName("PublicacionDebin")]
        [DisableConcurrentExecution(timeoutInSeconds: 1800)]
        [AutomaticRetry(Attempts = 3, OnAttemptsExceeded = AttemptsExceededAction.Delete)]
        public async Task PublishDebinTask(RegisterPublicationDTO publishDebinViewModel, Company currentCompany, int bankAccountId, User user, List<DetalleDeuda> debts)
        {
            try
            {
                int timeToAddInMinutesDefault = Convert.ToInt32(_configuration.GetSection("DebinDueTimeInMinutes").Value);

                RegistrarPublicacionResponse result = await _itauService.RegisterPublicationAsync(publishDebinViewModel);

                if (result.header.transaccion.estado.Equals("Exito"))
                {
                    string auxDebinCode = ((RegistrarPublicacionResponseType)result.body.Item).codigo;
                    // Validation for wrong length in debin code
                    if (auxDebinCode?.Length == 21)
                    {
                        auxDebinCode = auxDebinCode.Insert(0, "0");
                    }
                    var newDebinToInsert = new Debin()
                    {
                        Amount = publishDebinViewModel.Importe,
                        BankAccountId = bankAccountId,
                        VendorCuit = currentCompany.Cuit,
                        Currency = publishDebinViewModel.Moneda,
                        TransactionDate = publishDebinViewModel.Now,
                        IssueDate = publishDebinViewModel.Now,
                        ExpirationDate = publishDebinViewModel.Now.AddMinutes(timeToAddInMinutesDefault),
                        Payer = user,
                        Status = PaymentStatus.Pending,
                        Type = PaymentType.Normal,
                        DebinCode = auxDebinCode,
                        Source = PaymentSource.Itau,
                        ExternalCode = publishDebinViewModel.ExternalCodeString
                };


                    _debinRepository.Save(newDebinToInsert);

                    await _archivoDeudaRepository.SaveDebts(debts.Select(x=> x.Id).ToList(), newDebinToInsert.Id);
                }
            }
            catch (Exception ex)
            {
                Log.Error(ex, "PublishDebinTaskException debinVM:{@debinVM} user:{@user} company: {@company} bankAccountId: {@bankAccount} debts:{@debts}", publishDebinViewModel, user, currentCompany, bankAccountId, debts.Select(x => x.Id).ToArray());
                throw;
            }
            
        }

        public async Task<string> PublishExternDebin(ExternDebinViewModel debinData)
        {
            try {
                DateTime now = LocalDateTime.GetDateTimeNow();
                int timeToAddInMinutesDefault = Convert.ToInt32(_configuration.GetSection("DebinDueTimeInMinutes").Value);

                DateTime dueDate = now.AddMinutes(timeToAddInMinutesDefault);

                Company currentCompany = _companyRepository.GetByCuit(debinData.VendedorCuit);

                if (currentCompany == null)
                {
                    Log.Warning("PublishExternDebin: No se encontraron los datos del vendedor {company}", debinData.VendedorCuit);
                    throw new Exception("No se encontraron los datos del Vendedor " + debinData.VendedorCuit);
                }

                RegisterPublicationDTO externDebinData = new RegisterPublicationDTO()
                {
                    Vendedor = currentCompany,
                    CompradorCuit = debinData.CompradorCuit,
                    CompradorCbu = debinData.CBU,
                    CompradorNombre = debinData.CompradorNombre,
                    Producto = debinData.CodigoProducto,
                    Importe = debinData.Importe,
                    Moneda = debinData.Moneda,
                    DueDate = dueDate,
                    Now = now
                };

                var result = await _itauService.RegisterPublicationAsync(externDebinData);

                var debinCode = ((RegistrarPublicacionResponseType)result.body.Item).codigo;

                _anonymousPaymentRepository.Save(new AnonymousPayment()
                {
                    Amount = debinData.Importe,
                    CBU = debinData.CBU,
                    Cuit = debinData.CompradorCuit,
                    Currency = debinData.Moneda,
                    ExpirationDate = dueDate,
                    IssueDate = LocalDateTime.GetDateTimeNow(),
                    Migrated = false,
                    Status = PaymentStatus.Pending,
                    Type = PaymentType.Normal,
                    System = debinData.Sistema,
                    DebinCode = debinCode,
                    TransactionDate = LocalDateTime.GetDateTimeNow(),
                    VendorCuit = currentCompany.Cuit
                });

                return ((RegistrarPublicacionResponseType)result.body.Item).codigo;
            }
            catch (Exception ex)
            {
                Log.Error(ex, "ExternDebinException debinVM:{@debinData}", debinData);
                throw;
            }
        }


        public async Task<PaymentStatus> GetDebinState(GetDebinStateRequest debinStateRequest)
        {
            try
            {
                return await _itauService.GetDebinState(debinStateRequest);
            }
            catch (Exception ex)
            {
                Log.Error(ex, "GetDebinState Error.");
                throw;
            }
        }

        public async Task<PaymentStatus> GetDebinStatus(string debinCode)
        {
            var code =  await _anonymousPaymentRepository.GetByDebinCode(debinCode);
            return code.Status;
        }

        [AutomaticRetry(Attempts = 5, DelaysInSeconds = new []{ 300 }, OnAttemptsExceeded = AttemptsExceededAction.Delete)]
        [DisableConcurrentExecution(timeoutInSeconds: 1800)]
        public void CheckEveryDebinStateAndSendRequestOnStatusChanged(PerformContext context)
        {
            try
            {
                int retryCount = context.GetJobParameter<int>("RetryCount");
                // Foreach DEBIN, ask for state, compare with the one saved and call SGF
                List<Debin> debinesToBeUpdated = new List<Debin>() { };
                foreach (var debin in _debinRepository.GetAllPending())
                {
                    try
                    {
                        Company currentCompany = _companyRepository.GetByCuit(debin.VendorCuit);

                        PaymentStatus newState = AsyncHelper.RunSync(
                            async () => await GetDebinState(new GetDebinStateRequest()
                            {
                                Cuit = currentCompany.Cuit,
                                Cbu = (int)debin.Currency == 0 ? currentCompany.CbuPeso : currentCompany.CbuDolar,
                                CodigoDebin = string.IsNullOrEmpty(debin.DebinCode) ? debin.ExternalCode : debin.DebinCode,
                            }));

                        if (newState == PaymentStatus.Error && retryCount <= 5)
                        {
                            Log.Warning("Debin state with error: DebinId {debinId}, State {@state}", debin.Id, newState);
                        }

                        if (newState != debin.Status)
                        {
                            debin.Status = newState;

                            if (newState == PaymentStatus.Approved)
                            {
                                Log.Information("Se registro un pago debin aprobado," +
                                    "payment: {@debin}", debin);

                                // We only generate Files for the Debts with the same Currency than DEBIN
                                var debtsWithSameCurrencyThanDEBIN = debin.Debts.Where(x =>
                                    (Currency)Convert.ToInt32(x.CodigoMoneda) == debin.Currency).ToList();
                                if (debtsWithSameCurrencyThanDEBIN.Any())
                                {
                                    _paymentService.InformPaymentDone(
                                        debtsWithSameCurrencyThanDEBIN.OrderBy(x => x.FechaPrimerVenc));

                                    debin.Status = PaymentStatus.Informing;
                                    debin.InformedDate = LocalDateTime.GetDateTimeNow();
                                }
                                else
                                {
                                    Log.Information("No se encontrarón detalles deuda con moneda {moneda} para el pago, " +
                                        "payment: {@debin}", debin.Currency, debin);
                                }
                            }

                            debinesToBeUpdated.Add(debin);
                        }
                    }
                    catch (Exception ex)
                    {
                        Log.Error(ex, "Error proccesing DebinState for debinId: {debinId}", debin.Id);
                    }
                }

                _debinRepository.UpdateAll(debinesToBeUpdated);
                _notificationService.CheckForDebinNotifications(debinesToBeUpdated);

                //Updates extern debins
                List<AnonymousPayment> externDebines = new List<AnonymousPayment>();
                foreach (var externDebin in _anonymousPaymentRepository.GetAllPending())
                {
                    try
                    {
                        Company currentCompany = _companyRepository.GetByCuit(externDebin.VendorCuit);
                        GetDebinStateRequest debinExternStateRequest = new GetDebinStateRequest()
                        {
                            Cuit = currentCompany.Cuit,
                            Cbu = (int)externDebin.Currency == 0 ? currentCompany.CbuPeso : currentCompany.CbuDolar,
                            CodigoDebin = externDebin.DebinCode
                        };

                        PaymentStatus newState = AsyncHelper.RunSync(
                            async () => await GetDebinState(debinExternStateRequest));

                        if (newState == PaymentStatus.Error && retryCount <= 5)
                        {
                            Log.Warning("ExternDebin state with error: DebinId {debinId}, State {@state}", externDebin.Id, newState);
                        }

                        if (newState != externDebin.Status)
                        {
                            externDebin.Status = newState;
                            externDebines.Add(externDebin);
                        }
                    } catch(Exception ex) {
                        Log.Error(ex, "Error proccesing ExternDebinState for DebinId: {debinId}", externDebin.Id);
                    }
                }

                _anonymousPaymentRepository.UpdateAll(externDebines);
            }
            catch (Exception ex)
            {
                Log.Error(ex,"CheckEveryDebinStateAndSendRequestOnStatusChanged error");
                throw;
            }
        }

        public void InformPaymentDebinDoneManual(List<ManuallyInformPaymentDto> manuallyInformPayments)
        {
            try
            {
                var paymentMethods = _debinRepository.GetAllByIds(manuallyInformPayments.Select(x => x.Id).ToList());
                var debts = _archivoDeudaRepository.GetDetalleDeudasByDebinIds(manuallyInformPayments.Select(x => x.Id).ToList());
                var milliSecondsCount = 1000;

                foreach (var paymentMethod in paymentMethods)
                {
                    milliSecondsCount++;
                    var informDebts = debts.Where(x => x.PaymentMethodId == paymentMethod.Id && 
                                                       int.Parse(x.CodigoMoneda) == (int)paymentMethod.Currency).OrderBy(x => x.FechaPrimerVenc);

                    _paymentService.InformPaymentDone(
                        informDebts, 
                        manuallyInformPayments.Single(x => x.Id == paymentMethod.Id).FechaRecibo.AddMilliseconds(milliSecondsCount)
                    );

                    paymentMethod.Status = PaymentStatus.Informing;
                    paymentMethod.InformedDate = LocalDateTime.GetDateTimeNow();
                }

                _debinRepository.UpdateAll(paymentMethods);
            }
            catch (Exception ex)
            {
                Log.Error(ex, "InformPaymentDebinDoneManual(): Ocurrió un error al intentar informar los pagos: {@manuallyInformPayments}", manuallyInformPayments);
                throw;
            }
        }
    }
}