using nordelta.cobra.webapi.Services.Contracts;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Threading.Tasks;
using nordelta.cobra.webapi.Services.DTOs;
using RestSharp;
using Microsoft.Extensions.Options;
using nordelta.cobra.webapi.Configuration;
using nordelta.cobra.webapi.Controllers.Helpers;
using nordelta.cobra.webapi.Models;
using nordelta.cobra.webapi.Repositories.Contracts;
using nordelta.cobra.webapi.Models.ArchivoDeuda;
using Serilog;
using nordelta.cobra.webapi.Utils;
using Microsoft.Extensions.Caching.Distributed;
using Newtonsoft.Json;
using Hangfire;
using System.Text;
using Microsoft.Extensions.Configuration;
using nordelta.cobra.webapi.Helpers;
using Monitoreo = Nordelta.Monitoreo;

namespace nordelta.cobra.webapi.Services
{
    public class PaymentService : IPaymentService
    {
        private readonly IRestClient _restClient;
        private readonly IOptionsMonitor<ApiServicesConfig> _apiServicesConfig;
        private readonly IOptionsMonitor<TiposOperacionesConfiguration> _tiposOperacionesConfig;
        private readonly ServiciosMonitoreadosConfiguration _servicios;
        private readonly IArchivoDeudaRepository _archivoDeudaRepository;
        private readonly IAccountBalanceRepository _accountBalanceRepository;
        private readonly IDistributedCache _distributedCache;
        private readonly IUserRepository _userRepository;
        private readonly IContactDetailService _contactDetailService;
        private readonly IExchangeRateFileRepository _exchangeRateFileRepository;

        //cache key
        private readonly string _propertyCodesListKey = "propertyCodesList";
        private readonly string _propertyCodesListForAdvance = "propertyCodesListForAdvance";

        public readonly List<string> BusinessUnitsDisabledForAdvancePayments;

        public PaymentService(IRestClient restClient,
            IOptionsMonitor<ApiServicesConfig> options,
            IArchivoDeudaRepository archivoDeudaRepository, 
            IUserRepository userRepository, 
            IDistributedCache distributedCache, 
            IOptionsMonitor<TiposOperacionesConfiguration> tiposOperacionesConfiguration, 
            IOptions<ServiciosMonitoreadosConfiguration> servicesMonConfig, 
            IConfiguration configuration, 
            IAccountBalanceRepository accountBalanceRepository, 
            IContactDetailService contactDetailService,
            IExchangeRateFileRepository exchangeRateFileRepository
            )
        {
            _restClient = restClient;
            _apiServicesConfig = options;
            _tiposOperacionesConfig = tiposOperacionesConfiguration;
            _servicios = servicesMonConfig.Value;
            _archivoDeudaRepository = archivoDeudaRepository;
            _userRepository = userRepository;
            _accountBalanceRepository = accountBalanceRepository;
            _contactDetailService = contactDetailService;
            _exchangeRateFileRepository = exchangeRateFileRepository;
            _distributedCache = distributedCache;

            _restClient.Timeout = configuration.GetSection("RestSharpConfig:PaymentService:TimeoutInMinutes").Get<int>() * 60000; // Milliseconds
            _restClient.ThrowOnAnyError = true;

            BusinessUnitsDisabledForAdvancePayments = configuration.GetSection("BusinessUnitsDisabledForAdvancePayments").Get<List<string>>();
        }

        public List<ResumenCuentaResponse> GetPaymentsSummary(string cuit, string currentAccount)
        {
            string productCode = GetProductCode(currentAccount);
            if (!string.IsNullOrEmpty(productCode))
            {
                _restClient.BaseUrl = new Uri(_apiServicesConfig.Get(ApiServicesConfig.SgfApi).Url);
                RestRequest request = new RestRequest("/CuentaCorriente/ObtenerResumenes", Method.GET);
                request.AddHeader("Token", _apiServicesConfig.Get(ApiServicesConfig.SgfApi).Token);
                request.AddParameter("CuentaCorriente", productCode);
                request.AddParameter("Cuit", cuit);



                IRestResponse<List<ResumenCuentaResponse>> paymentsResponse = _restClient.Execute<List<ResumenCuentaResponse>>(request);

                Log.Debug("Se trae Resumen de cuenta. \n Cuit: {cuit} \n CodigoProducto: {productCode} \n RequestUrl: {url} \n ResponseData: {@response}", cuit, productCode, request.Resource, paymentsResponse.Data);
                if (!paymentsResponse.IsSuccessful)
                {
                    Log.Error("No se pudo obtener Resumen de cuenta.\n Cuit:{cuit} \n Producto: {producto} \n Request: {@request} \n Response: {@response}", cuit, productCode, request, paymentsResponse);
                    return null;
                }

                return paymentsResponse.Data;
            }
            else
            {
                return null;
            }
        }

        public List<BalanceDetailDto> GetRawBalanceDetail(string cuit, string productCode)
        {
            var requestModel = new ResumenCuentaRequest();
            if (!string.IsNullOrEmpty(cuit))
            {
                requestModel.TipoDocumento = "CUIT";
                requestModel.NroDocumentos.Add(cuit);
            }

            _restClient.BaseUrl = new Uri(_apiServicesConfig.Get(ApiServicesConfig.SgfApi).Url);
            RestRequest request = new RestRequest("/Deuda/ObtenerDetalleSaldos", Method.POST);
            request.AddHeader("Token", _apiServicesConfig.Get(ApiServicesConfig.SgfApi).Token);
            request.AddJsonBody(requestModel);

            IRestResponse<List<BalanceDetailDto>> paymentsResponse = _restClient.Execute<List<BalanceDetailDto>>(request);
            if (!paymentsResponse.IsSuccessful)
            {
                Monitoreo.Monitor.Critical("GetRawBalanceDetail(): Ocurrió un error al intentar obtener Detalle de Balances de Oracle", _servicios.ReportesOracle);
                Log.Error("No se pudo obtener Detalle de Balances.\n Cuit:{cuit} \n CodProducto: {productCode} \n Request: {@request} \n Response: {@response}", cuit, productCode, request, paymentsResponse);
            }
            else
            {
                Monitoreo.Monitor.Ok("GetRawBalanceDetail(): Se obtuvo el Detalle de Balances de Oracle", _servicios.ReportesOracle);
            }

            List<BalanceDetailDto> response = paymentsResponse.Data;

            List<BalanceDetailDto> filteredResponse = response.Where(x => x.Producto == productCode).ToList();

            return filteredResponse;
        }

        public List<ResumenCuentaResponse> GetBalanceDetail(string cuit, string productCode)
        {
            var requestModel = new ResumenCuentaRequest();
            if (!string.IsNullOrEmpty(cuit))
            {
                requestModel.TipoDocumento = "CUIT";
                requestModel.NroDocumentos.Add(cuit);
            }

            _restClient.BaseUrl = new Uri(_apiServicesConfig.Get(ApiServicesConfig.SgfApi).Url);
            RestRequest request = new RestRequest("/Deuda/ObtenerDetalleSaldos", Method.POST);
            request.AddHeader("Token", _apiServicesConfig.Get(ApiServicesConfig.SgfApi).Token);
            request.AddJsonBody(requestModel);

            IRestResponse<List<BalanceDetailDto>> paymentsResponse = _restClient.Execute<List<BalanceDetailDto>>(request);
            if (!paymentsResponse.IsSuccessful)
            {
                Monitoreo.Monitor.Critical("GetBalanceDetail(): Ocurrió un error al intentar obtener Detalle de Balances de Oracle", _servicios.ReportesOracle);
                Log.Error("No se pudo obtener Detalle de Balances.\n Cuit:{cuit} \n CodProducto: {productCode} \n Request: {@request} \n Response: {@response}", cuit, productCode, request, paymentsResponse);
            }
            else
            {
                Monitoreo.Monitor.Ok("GetBalanceDetail(): Se obtuvo el Detalle de Balances de Oracle", _servicios.ReportesOracle);
            }

            List<BalanceDetailDto> response = paymentsResponse.Data;

            List<BalanceDetailDto> filteredResponse = response
                .Where(x => x.Producto == productCode)
                .ToList();


            List<DetalleDeuda> deudas = GetAllPayments(cuit);
            List<ResumenCuentaResponse> result = new List<ResumenCuentaResponse>();

            if (filteredResponse.Any())
            {
                result = filteredResponse.Select(x => new ResumenCuentaResponse()
                {
                    Fecha = x.DueDate,
                    Moneda = x.CurrencyCode,
                    Saldo = x.AmountDueRemaining,
                    TipoOperacion = x.DocType,
                    Intereses = x.AmountAdjusted,
                    Capital = x.AmountLineItemsRemaining,
                    TrxId = x.TrxId,
                    FacElect = x.FacElect,
                    TrxNumber = x.TrxNumber,
                    Producto = x.Producto,
                    Cuit = x.CustomerDocumentNumber
                }).ToList();
            }

            foreach (var res in result)
            {
                DateTime fechaCuota = DateTime.ParseExact(res.Fecha, "dd/MM/yyyy", CultureInfo.InvariantCulture).Date;

                foreach (var deuda in deudas)
                {
                    DateTime fechaDeuda = DateTime.ParseExact(deuda.FechaPrimerVenc, "yyyyMMdd", CultureInfo.InvariantCulture).Date;
                    try
                    {
                        if (deuda.ObsLibreCuarta == res.Producto && fechaDeuda.Equals(fechaCuota))
                        {
                            res.OnDebtDetail = true;
                            res.Processing = false;

                            if (deuda.PaymentMethodId != null)
                            {
                                if (deuda.PaymentMethod.GetType().Name == nameof(Debin))
                                {
                                    var debin = _archivoDeudaRepository.GetDebinByPaymentMethodId((int)deuda.PaymentMethodId);
                                    res.Processing = (debin.Status == PaymentStatus.Pending) || (debin.Status == PaymentStatus.Approved || (debin.Status == PaymentStatus.Rejected));
                                }

                                if (deuda.PaymentMethod.GetType().Name == nameof(CvuOperation) ||
                                    deuda.PaymentMethod.GetType().Name == nameof(Echeq))
                                {
                                    res.Processing = true;
                                }
                            }
                            else
                            {
                                res.Processing = deuda.PaymentReportId != null;
                            }
                        }
                    }
                    catch (Exception ex)
                    {
                        Monitoreo.Monitor.Warning("GetBalanceDetail(): Error al intentar obtener DetalleDeuda y estado de procesamiento", _servicios.ReportesOracle);
                        Log.Error(ex, $"GetBalanceDetail: Error al intentar obtener DetalleDeuda y estado de procesamiento para la siguiente cuota: (Cuit: {res.Cuit}, Fecha: {res.Fecha}, Producto: {res.Producto}).");
                    }
                }
            }

            return result;
        }

        public List<ResumenCuentaResponse> GetBalanceDetail(string cuit, string productCode, string clientReference = "")
        {
            var requestModel = new ResumenCuentaRequest();
            if (!string.IsNullOrEmpty(cuit))
            {
                requestModel.TipoDocumento = "CUIT";
                requestModel.NroDocumentos.Add(cuit);
            }

            _restClient.BaseUrl = new Uri(_apiServicesConfig.Get(ApiServicesConfig.SgfApi).Url);
            RestRequest request = new RestRequest("/Deuda/ObtenerDetalleSaldos", Method.POST);
            request.AddHeader("Token", _apiServicesConfig.Get(ApiServicesConfig.SgfApi).Token);
            request.AddJsonBody(requestModel);

            IRestResponse<List<BalanceDetailDto>> paymentsResponse = _restClient.Execute<List<BalanceDetailDto>>(request);
            if (!paymentsResponse.IsSuccessful)
            {
                Monitoreo.Monitor.Critical("GetBalanceDetail(): Ocurrió un error al intentar obtener Detalle de Balances de Oracle", _servicios.ReportesOracle);
                Log.Error("No se pudo obtener Detalle de Balances.\n Cuit:{cuit} \n CodProducto: {productCode} \n Request: {@request} \n Response: {@response}", cuit, productCode, request, paymentsResponse);
            }
            else
            {
                Monitoreo.Monitor.Ok("GetBalanceDetail(): Se obtuvo el Detalle de Balances de Oracle", _servicios.ReportesOracle);
            }

            List<BalanceDetailDto> response = paymentsResponse.Data;

            List<BalanceDetailDto> filteredResponse = response
                .Where(x => x.Producto == productCode && x.ReferenciaCliente == clientReference)
                .ToList();


            List<DetalleDeuda> deudas = GetAllPayments(cuit);
            List<ResumenCuentaResponse> result = new List<ResumenCuentaResponse>();

            if (filteredResponse.Any())
            {
                result = filteredResponse.Select(x => new ResumenCuentaResponse()
                {
                    Fecha = x.DueDate,
                    Moneda = x.CurrencyCode,
                    Saldo = x.AmountDueRemaining,
                    TipoOperacion = x.DocType,
                    Intereses = x.AmountAdjusted,
                    Capital = x.AmountLineItemsRemaining,
                    TrxId = x.TrxId,
                    FacElect = x.FacElect,
                    TrxNumber = x.TrxNumber,
                    Producto = x.Producto,
                    Cuit = x.CustomerDocumentNumber
                }).ToList();
            }

            foreach (var res in result)
            {
                DateTime fechaCuota = DateTime.ParseExact(res.Fecha, "dd/MM/yyyy", CultureInfo.InvariantCulture).Date;

                foreach (var deuda in deudas)
                {
                    DateTime fechaDeuda = DateTime.ParseExact(deuda.FechaPrimerVenc, "yyyyMMdd", CultureInfo.InvariantCulture).Date;
                    try
                    {
                        if (deuda.ObsLibreCuarta == res.Producto && fechaDeuda.Equals(fechaCuota))
                        {
                            res.OnDebtDetail = true;
                            res.Processing = false;

                            if (deuda.PaymentMethodId != null)
                            {
                                if (deuda.PaymentMethod.GetType().Name == nameof(Debin))
                                {
                                    var debin = _archivoDeudaRepository.GetDebinByPaymentMethodId((int)deuda.PaymentMethodId);
                                    res.Processing = (debin.Status == PaymentStatus.Pending) || (debin.Status == PaymentStatus.Approved || (debin.Status == PaymentStatus.Rejected));
                                }

                                if (deuda.PaymentMethod.GetType().Name == nameof(CvuOperation) ||
                                    deuda.PaymentMethod.GetType().Name == nameof(Echeq))
                                {
                                    res.Processing = true;
                                }
                            }
                            else
                            {
                                res.Processing = deuda.PaymentReportId != null;
                            }
                        }
                    }
                    catch (Exception ex)
                    {
                        Monitoreo.Monitor.Warning("GetBalanceDetail(): Error al intentar obtener DetalleDeuda y estado de procesamiento", _servicios.ReportesOracle);
                        Log.Error(ex, $"GetBalanceDetail: Error al intentar obtener DetalleDeuda y estado de procesamiento para la siguiente cuota: (Cuit: {res.Cuit}, Fecha: {res.Fecha}, Producto: {res.Producto}).");
                    }
                }
            }

            return result;
        }

        public List<PaymentHistoryDto> GetApplicationDetail(string cuit, string productCode)
        {
            var requestModel = new ResumenCuentaRequest();
            if (!string.IsNullOrEmpty(cuit))
            {
                requestModel.TipoDocumento = "CUIT";
                requestModel.NroDocumentos.Add(cuit);
            }

            _restClient.BaseUrl = new Uri(_apiServicesConfig.Get(ApiServicesConfig.SgfApi).Url);
            RestRequest request = new RestRequest("/Deuda/ObtenerDetalleAplicaciones", Method.POST);
            request.AddHeader("Token", _apiServicesConfig.Get(ApiServicesConfig.SgfApi).Token);
            request.AddJsonBody(requestModel);

            IRestResponse<List<ApplicationDetailDto>> paymentsResponse = _restClient.Execute<List<ApplicationDetailDto>>(request);
            if (!paymentsResponse.IsSuccessful)
            {
                Monitoreo.Monitor.Critical("GetApplicationDetail(): Ocurrió un error al intentar obtener Detalle de Aplicaciones de Oracle", _servicios.ReportesOracle);
                Log.Error("No se pudo obtener Detalle de Aplicaciones.\n Cuit:{cuit} \n CodProducto: {productCode} \n Request: {@request} \n Response: {@response}", cuit, productCode, request, paymentsResponse);
            }
            else
            {
                Monitoreo.Monitor.Ok("GetApplicationDetail(): Se obtuvo Detalle de Aplicaciones de Oracle", _servicios.ReportesOracle);
            }

            List<ApplicationDetailDto> response = paymentsResponse.Data;

            List<ApplicationDetailDto> filteredResponse = response
                .Where(x => x.Producto == productCode)
                .ToList();

            var groupedList = filteredResponse.GroupBy(x => new { x.InvoiceCurrencyCode, x.AmounDueOriginal, x.AmountDueRemaining, x.DueDate, x.TrxNumber }).ToList();

            List<PaymentHistoryDto> result = new List<PaymentHistoryDto>();

            if (filteredResponse.Any())
            {
                result = groupedList.Select(x => new PaymentHistoryDto()
                {
                    Moneda = x.Key.InvoiceCurrencyCode,
                    Saldo = x.Key.AmountDueRemaining,
                    FechaVenc = x.Key.DueDate,
                    Importe = x.Key.AmounDueOriginal,
                    Details = x.Select(y => new PaymentHistoryDetailDto()
                    {
                        Fecha = y.DocDate,
                        Moneda = y.DocCurrencyCode,
                        Importe = y.AmountAppliedFrom,
                        MonedaFC = y.InvoiceCurrencyCode,
                        ImporteFC = y.AmountApplied,
                        Tipo = y.DocType,
                        ApplicationType = y.ApplicationType,
                        BuId = y.BuId,
                        DocId = y.DocId,
                        LegalEntityId = y.LegalEntityId,
                        DocNumber = y.DocNumber,
                        TrxId = y.TrxId,
                        FacElect = y.FacElect,
                        TrxNumber = y.TrxNumber,
                        ApplTc = y.ApplTc,
                        DocTc = y.DocTc
                    }).ToList()
                }).ToList();

                result.ForEach(x => x.Details = x.Details.OrderBy(y => DateTime.ParseExact(y.Fecha, "dd/MM/yyyy", CultureInfo.InvariantCulture)).ToList());
            }

            return result;
        }
        public List<PaymentHistoryDto> GetApplicationDetail(string cuit, string productCode, string clientReference = "")
        {
            var requestModel = new ResumenCuentaRequest();
            if (!string.IsNullOrEmpty(cuit))
            {
                requestModel.TipoDocumento = "CUIT";
                requestModel.NroDocumentos.Add(cuit);
            }

            _restClient.BaseUrl = new Uri(_apiServicesConfig.Get(ApiServicesConfig.SgfApi).Url);
            RestRequest request = new RestRequest("/Deuda/ObtenerDetalleAplicaciones", Method.POST);
            request.AddHeader("Token", _apiServicesConfig.Get(ApiServicesConfig.SgfApi).Token);
            request.AddJsonBody(requestModel);

            IRestResponse<List<ApplicationDetailDto>> paymentsResponse = _restClient.Execute<List<ApplicationDetailDto>>(request);
            if (!paymentsResponse.IsSuccessful)
            {
                Monitoreo.Monitor.Critical("GetApplicationDetail(): Ocurrió un error al intentar obtener Detalle de Aplicaciones de Oracle", _servicios.ReportesOracle);
                Log.Error("No se pudo obtener Detalle de Aplicaciones.\n Cuit:{cuit} \n CodProducto: {productCode} \n Request: {@request} \n Response: {@response}", cuit, productCode, request, paymentsResponse);
            }
            else
            {
                Monitoreo.Monitor.Ok("GetApplicationDetail(): Se obtuvo Detalle de Aplicaciones de Oracle", _servicios.ReportesOracle);
            }

            List<ApplicationDetailDto> response = paymentsResponse.Data;

            List<ApplicationDetailDto> filteredResponse = response
                .Where(x => x.Producto == productCode && x.ReferenciaCliente == clientReference)
                .ToList();

            var groupedList = filteredResponse.GroupBy(x => new { x.InvoiceCurrencyCode, x.AmounDueOriginal, x.AmountDueRemaining, x.DueDate, x.TrxNumber }).ToList();

            List<PaymentHistoryDto> result = new List<PaymentHistoryDto>();

            if (filteredResponse.Any())
            {
                result = groupedList.Select(x => new PaymentHistoryDto()
                {
                    Moneda = x.Key.InvoiceCurrencyCode,
                    Saldo = x.Key.AmountDueRemaining,
                    FechaVenc = x.Key.DueDate,
                    Importe = x.Key.AmounDueOriginal,
                    Details = x.Select(y => new PaymentHistoryDetailDto()
                    {
                        Fecha = y.DocDate,
                        Moneda = y.DocCurrencyCode,
                        Importe = y.AmountAppliedFrom,
                        MonedaFC = y.InvoiceCurrencyCode,
                        ImporteFC = y.AmountApplied,
                        Tipo = y.DocType,
                        ApplicationType = y.ApplicationType,
                        BuId = y.BuId,
                        DocId = y.DocId,
                        LegalEntityId = y.LegalEntityId,
                        DocNumber = y.DocNumber,
                        TrxId = y.TrxId,
                        FacElect = y.FacElect,
                        TrxNumber = y.TrxNumber,
                        ApplTc = y.ApplTc,
                        DocTc = y.DocTc
                    }).ToList()
                }).ToList();

                result.ForEach(x => x.Details = x.Details.OrderBy(y => DateTime.ParseExact(y.Fecha, "dd/MM/yyyy", CultureInfo.InvariantCulture)).ToList());
            }

            return result;
        }


        public List<CuitProductBuDto> GetUnappliedProductList(string userCuit)
        {
            var requestModel = new ResumenCuentaRequest();
            if (!string.IsNullOrEmpty(userCuit))
            {
                requestModel.TipoDocumento = "CUIT";
                requestModel.NroDocumentos.Add(userCuit);
            }

            _restClient.BaseUrl = new Uri(_apiServicesConfig.Get(ApiServicesConfig.SgfApi).Url);
            RestRequest request = new RestRequest("/Deuda/ObtenerCobrosNoAplicados", Method.POST);
            request.AddHeader("Token", _apiServicesConfig.Get(ApiServicesConfig.SgfApi).Token);
            request.AddJsonBody(requestModel);

            try
            {
                IRestResponse<List<DetailedUnappliedPayment>> paymentsResponse = _restClient.Execute<List<DetailedUnappliedPayment>>(request);
                if (!paymentsResponse.IsSuccessful)
                {
                    Monitoreo.Monitor.Critical("GetUnappliedProductList(): Ocurrió un error al intentar obtener Cobros No Aplicados de Oracle", _servicios.ReportesOracle);
                    Log.Error("No se pudo obtener Cobros No Aplicados.\n Cuit:{cuit}\n Request: {@request} \n Response: {@response}", userCuit, request, paymentsResponse);
                }
                else
                {
                    Monitoreo.Monitor.Ok("GetUnappliedProductList(): Se obtuvo Cobros No Aplicados de Oracle", _servicios.ReportesOracle);
                }

                var result = new List<CuitProductBuDto>();

                if (paymentsResponse.Data != null)
                {
                    result = paymentsResponse.Data.GroupBy(l => new { l.CustomerDocumentNumber, l.Producto, l.BusinessUnitName }).Select(cl =>
                        new CuitProductBuDto()
                        {
                            Cuit = cl.Key.CustomerDocumentNumber,
                            Product = cl.Key.Producto,
                            BusinessUnit = cl.Key.BusinessUnitName
                        }).ToList();
                }

                return result;
            }
            catch (Exception ex)
            {
                Monitoreo.Monitor.Critical("GetUnappliedProductList(): Error al obtener los cobros no aplicados del Sgf", _servicios.ReportesOracle);

                Log.Error(ex, @"GetUnappliedProductList, 
                    Type:GetUnappliedProductListError,
                    Description: Error fetching UnappliedProductList:
                    request: {@request}", request);
                throw;
            }
        }

        public async Task<List<CuitProductBuDto>> GetUnappliedProductListAsync(string userCuit)
        {
            var requestModel = new ResumenCuentaRequest();
            if (!string.IsNullOrEmpty(userCuit))
            {
                requestModel.TipoDocumento = "CUIT";
                requestModel.NroDocumentos.Add(userCuit);
            }

            _restClient.BaseUrl = new Uri(_apiServicesConfig.Get(ApiServicesConfig.SgfApi).Url);
            RestRequest request = new RestRequest("/Deuda/ObtenerCobrosNoAplicados", Method.POST);
            request.AddHeader("Token", _apiServicesConfig.Get(ApiServicesConfig.SgfApi).Token);
            request.AddJsonBody(requestModel);

            try
            {
                IRestResponse<List<DetailedUnappliedPayment>> paymentsResponse = await _restClient.ExecuteAsync<List<DetailedUnappliedPayment>>(request);
                if (!paymentsResponse.IsSuccessful)
                {
                    Monitoreo.Monitor.Critical("GetUnappliedProductListAsync(): Ocurrió un error al intentar obtener Cobros No Aplicados de Oracle", _servicios.ReportesOracle);
                    Log.Error("No se pudo obtener Cobros No Aplicados.\n Cuit:{cuit}\n Request: {@request} \n Response: {@response}", userCuit, request, paymentsResponse);
                }
                else
                {
                    Monitoreo.Monitor.Ok("GetUnappliedProductListAsync(): Se obtuvo Cobros No Aplicados de Oracle", _servicios.ReportesOracle);
                }

                var result = new List<CuitProductBuDto>();

                if (paymentsResponse.Data != null)
                {
                    result = paymentsResponse.Data.GroupBy(l => new { l.CustomerDocumentNumber, l.Producto, l.BusinessUnitName, l.ReferenciaCliente}).Select(cl =>
                        new CuitProductBuDto()
                        {
                            Cuit = cl.Key.CustomerDocumentNumber,
                            Product = cl.Key.Producto,
                            BusinessUnit = cl.Key.BusinessUnitName,
                            ReferenciaCliente = cl.Key.ReferenciaCliente
                        }).ToList();
                }

                return result;
            }
            catch (Exception ex)
            {
                Monitoreo.Monitor.Critical("GetUnappliedProductListAsync(): Error al obtener los cobros no aplicados del Sgf", _servicios.ReportesOracle);

                Log.Error(ex, @"GetUnappliedProductListAsync, 
                    Type:GetUnappliedProductListAsyncError,
                    Description: Error fetching UnappliedProductList:
                    request: {@request}", request);
                throw;
            }
        }
        public List<CuitProductCurrencyDto> GetUnappliedProductList(List<string> userCuits)
        {
            var requestModel = new ResumenCuentaRequest();
            requestModel.TipoDocumento = "CUIT";
            requestModel.NroDocumentos = userCuits;

            _restClient.BaseUrl = new Uri(_apiServicesConfig.Get(ApiServicesConfig.SgfApi).Url);
            RestRequest request = new RestRequest("/Deuda/ObtenerCobrosNoAplicados", Method.POST);
            request.AddHeader("Token", _apiServicesConfig.Get(ApiServicesConfig.SgfApi).Token);
            request.AddJsonBody(requestModel);
            try
            {
                IRestResponse<List<DetailedUnappliedPayment>> paymentsResponse = _restClient.Execute<List<DetailedUnappliedPayment>>(request);
                if (!paymentsResponse.IsSuccessful)
                {
                    Monitoreo.Monitor.Critical("GetUnappliedProductList(): Ocurrió un error al intentar obtener Cobros No Aplicados de Oracle", _servicios.ReportesOracle);
                    Log.Error("No se pudo obtener Cobros No Aplicados.\n Cuit:{cuit}\n Request: {@request} \n Response: {@response}", userCuits, request, paymentsResponse);
                }
                else
                {
                    Monitoreo.Monitor.Ok("GetUnappliedProductList(): Se obtuvo Cobros No Aplicados de Oracle", _servicios.ReportesOracle);
                }

                var result = new List<CuitProductCurrencyDto>();

                if (paymentsResponse.Data != null)
                {
                    result = paymentsResponse.Data.GroupBy(l => new { l.CustomerDocumentNumber, l.Producto }).Select(cl =>
                        new CuitProductCurrencyDto()
                        {
                            Cuit = cl.Key.CustomerDocumentNumber,
                            Product = cl.Key.Producto
                        }).ToList();
                }

                return result;
            }
            catch (Exception ex)
            {
                Monitoreo.Monitor.Critical("GetUnappliedProductList(): Error al obtener los cobros no aplicados del Sgf", _servicios.ReportesOracle);

                Log.Error(ex, @"GetUnappliedProductList, 
                    Type:GetUnappliedProductListError,
                    Description: Error fetching UnappliedProductList:
                    request: {@request}", request);
                throw;
            }
        }

        public List<CuitProductBuDto> GetBalanceDetailProductList(string userCuit)
        {
            var requestModel = new ResumenCuentaRequest();
            if (!string.IsNullOrEmpty(userCuit))
            {
                requestModel.TipoDocumento = "CUIT";
                requestModel.NroDocumentos.Add(userCuit);
            }

            _restClient.BaseUrl = new Uri(_apiServicesConfig.Get(ApiServicesConfig.SgfApi).Url);
            RestRequest request = new RestRequest("/Deuda/ObtenerDetalleSaldos", Method.POST);
            request.AddHeader("Token", _apiServicesConfig.Get(ApiServicesConfig.SgfApi).Token);
            request.AddJsonBody(requestModel);
            try
            {
                IRestResponse<List<BalanceDetailDto>> paymentsResponse = _restClient.Execute<List<BalanceDetailDto>>(request);
                if (!paymentsResponse.IsSuccessful)
                {
                    Monitoreo.Monitor.Critical("GetBalanceDetailProductList(): Ocurrió un error al intentar obtener Detalle de Balances de Oracle", _servicios.ReportesOracle);
                    Log.Error("No se pudo obtener el Detalle de Balances.\n Cuit:{cuit}\n Request: {@request} \n Response: {@response}", userCuit, request, paymentsResponse);
                }
                else
                {
                    Monitoreo.Monitor.Ok("GetBalanceDetailProductList(): Se obtuvo Detalle de Balances de Oracle", _servicios.ReportesOracle);
                }

                var result = new List<CuitProductBuDto>();

                if (paymentsResponse.Data != null)
                {
                    result = paymentsResponse.Data.GroupBy(l => new { l.CustomerDocumentNumber, l.Producto, l.BusinessUnitName }).Select(cl =>
                        new CuitProductBuDto()
                        {
                            Cuit = cl.Key.CustomerDocumentNumber,
                            Product = cl.Key.Producto,
                            BusinessUnit = cl.Key.BusinessUnitName
                        }).ToList();
                }

                return result;
            }
            catch (Exception ex)
            {
                Monitoreo.Monitor.Critical("GetBalanceDetailProductList(): Error al obtener los detalles saldo del Sgf", _servicios.ReportesOracle);

                Log.Error(ex, @"GetBalanceDetailProductList, 
                    Type:GetBalanceDetailProductListError,
                    Description: Error fetching BalanceDetailProductList:
                    request: {@request}", request);
                throw;
            }
        }

        public async Task<List<CuitProductBuDto>> GetBalanceDetailProductListAsync(string userCuit)
        {
            var requestModel = new ResumenCuentaRequest();
            if (!string.IsNullOrEmpty(userCuit))
            {
                requestModel.TipoDocumento = "CUIT";
                requestModel.NroDocumentos.Add(userCuit);
            }

            _restClient.BaseUrl = new Uri(_apiServicesConfig.Get(ApiServicesConfig.SgfApi).Url);
            RestRequest request = new RestRequest("/Deuda/ObtenerDetalleSaldos", Method.POST);
            request.AddHeader("Token", _apiServicesConfig.Get(ApiServicesConfig.SgfApi).Token);
            request.AddJsonBody(requestModel);
            try
            {
                IRestResponse<List<BalanceDetailDto>> paymentsResponse = await _restClient.ExecuteAsync<List<BalanceDetailDto>>(request);
                if (!paymentsResponse.IsSuccessful)
                {
                    Monitoreo.Monitor.Critical("GetBalanceDetailProductListAsync(): Ocurrió un error al intentar obtener Detalle de Balances de Oracle", _servicios.ReportesOracle);
                    Log.Error("No se pudo obtener el Detalle de Balances.\n Cuit:{cuit}\n Request: {@request} \n Response: {@response}", userCuit, request, paymentsResponse);
                }
                else
                {
                    Monitoreo.Monitor.Ok("GetBalanceDetailProductListAsync(): Se obtuvo Detalle de Balances de Oracle", _servicios.ReportesOracle);
                }

                var result = new List<CuitProductBuDto>();

                if (paymentsResponse.Data != null)
                {
                    result = paymentsResponse.Data.GroupBy(l => new { l.CustomerDocumentNumber, l.Producto, l.BusinessUnitName, l.ReferenciaCliente, l.ReferenciaDomicilioCliente }).Select(cl =>
                        new CuitProductBuDto()
                        {
                            Cuit = cl.Key.CustomerDocumentNumber,
                            Product = cl.Key.Producto,
                            BusinessUnit = cl.Key.BusinessUnitName,
                            ReferenciaCliente = cl.Key.ReferenciaCliente
                        }).ToList();
                }

                return result;
            }
            catch (Exception ex)
            {
                Monitoreo.Monitor.Critical("GetBalanceDetailProductListAsync(): Error al obtener los detalles saldo del Sgf", _servicios.ReportesOracle);

                Log.Error(ex, @"GetBalanceDetailProductListAsync, 
                    Type:GetBalanceDetailProductListAsyncError,
                    Description: Error fetching BalanceDetailProductList:
                    request: {@request}", request);
                throw;
            }
        }

        public List<CuitProductCurrencyDto> GetBalanceDetailProductList(List<string> userCuits)
        {
            var requestModel = new ResumenCuentaRequest();
            requestModel.TipoDocumento = "CUIT";
            requestModel.NroDocumentos = userCuits;

            _restClient.BaseUrl = new Uri(_apiServicesConfig.Get(ApiServicesConfig.SgfApi).Url);
            RestRequest request = new RestRequest("/Deuda/ObtenerDetalleSaldos", Method.POST);
            request.AddHeader("Token", _apiServicesConfig.Get(ApiServicesConfig.SgfApi).Token);
            request.AddJsonBody(requestModel);
            try
            {
                IRestResponse<List<BalanceDetailDto>> paymentsResponse = _restClient.Execute<List<BalanceDetailDto>>(request);
                if (!paymentsResponse.IsSuccessful)
                {
                    Monitoreo.Monitor.Critical("GetBalanceDetailProductList(): Ocurrió un error al intentar obtener Detalle de Balances de Oracle", _servicios.ReportesOracle);
                    Log.Error("No se pudo obtener el Detalle de Balances.\n Cuit:{cuit}\n Request: {@request} \n Response: {@response}", userCuits, request, paymentsResponse);
                }
                else
                {
                    Monitoreo.Monitor.Ok("GetBalanceDetailProductList(): Se obtuvo Detalle de Balances de Oracle", _servicios.ReportesOracle);
                }

                var result = new List<CuitProductCurrencyDto>();

                if (paymentsResponse.Data != null)
                {
                    result = paymentsResponse.Data.GroupBy(l => new { l.CustomerDocumentNumber, l.Producto }).Select(cl =>
                        new CuitProductCurrencyDto()
                        {
                            Cuit = cl.Key.CustomerDocumentNumber,
                            Product = cl.Key.Producto
                        }).ToList();
                }

                return result;
            }
            catch (Exception ex)
            {
                Monitoreo.Monitor.Critical("GetBalanceDetailProductList(): Error al obtener los detalles saldo del Sgf", _servicios.ReportesOracle);

                Log.Error(ex, @"GetBalanceDetailProductList, 
                    Type:GetBalanceDetailProductListError,
                    Description: Error fetching BalanceDetailProductList:
                    request: {@request}", request);
                throw;
            }
        }


        public List<CuitProductBuDto> GetApplicationDetailProductList(string userCuit)
        {
            var requestModel = new ResumenCuentaRequest();
            if (!string.IsNullOrEmpty(userCuit))
            {
                requestModel.TipoDocumento = "CUIT";
                requestModel.NroDocumentos.Add(userCuit);
            }

            _restClient.BaseUrl = new Uri(_apiServicesConfig.Get(ApiServicesConfig.SgfApi).Url);
            RestRequest request = new RestRequest("/Deuda/ObtenerDetalleAplicaciones", Method.POST);
            request.AddHeader("Token", _apiServicesConfig.Get(ApiServicesConfig.SgfApi).Token);
            request.AddJsonBody(requestModel);
            try
            {

                IRestResponse<List<ApplicationDetailDto>> paymentsResponse = _restClient.Execute<List<ApplicationDetailDto>>(request);
                if (!paymentsResponse.IsSuccessful)
                {
                    Monitoreo.Monitor.Critical("GetApplicationDetailProductList(): Ocurrió un error al intentar obtener Detalle de Aplicaciones de Oracle", _servicios.ReportesOracle);
                    Log.Error("No se pudo obtener el Detalle de Aplicaciones.\n Cuit:{cuit}\n Request: {@request} \n Response: {@response}", userCuit, request, paymentsResponse);
                }
                else
                {
                    Monitoreo.Monitor.Ok("GetApplicationDetailProductList(): Se obtuvo Detalle de Aplicaciones de Oracle", _servicios.ReportesOracle);
                }

                var result = new List<CuitProductBuDto>();

                if (paymentsResponse.Data != null)
                {
                    result = paymentsResponse.Data.GroupBy(l => new { l.CustomerDocumentNumber, l.Producto, l.BusinessUnitName }).Select(cl =>
                        new CuitProductBuDto()
                        {
                            Cuit = cl.Key.CustomerDocumentNumber,
                            Product = cl.Key.Producto,
                            BusinessUnit = cl.Key.BusinessUnitName
                        }).ToList();
                }

                return result;
            }
            catch (Exception ex)
            {
                Monitoreo.Monitor.Critical("GetApplicationDetailProductList(): Error al obtener los detalles de aplicaciones del Sgf", _servicios.ReportesOracle);

                Log.Error(ex, @"GetApplicationDetailProductList, 
                    Type:GetApplicationDetailProductListError,
                    Description: Error fetching ApplicationDetailProductList:
                    request: {@request}", request);
                throw;
            }
        }

        public async Task<List<CuitProductBuDto>> GetApplicationDetailProductListAsync(string userCuit)
        {
            var requestModel = new ResumenCuentaRequest();
            if (!string.IsNullOrEmpty(userCuit))
            {
                requestModel.TipoDocumento = "CUIT";
                requestModel.NroDocumentos.Add(userCuit);
            }

            _restClient.BaseUrl = new Uri(_apiServicesConfig.Get(ApiServicesConfig.SgfApi).Url);
            RestRequest request = new RestRequest("/Deuda/ObtenerDetalleAplicaciones", Method.POST);
            request.AddHeader("Token", _apiServicesConfig.Get(ApiServicesConfig.SgfApi).Token);
            request.AddJsonBody(requestModel);
            try
            {

                IRestResponse<List<ApplicationDetailDto>> paymentsResponse = await _restClient.ExecuteAsync<List<ApplicationDetailDto>>(request);
                if (!paymentsResponse.IsSuccessful)
                {
                    Monitoreo.Monitor.Critical("GetApplicationDetailProductListAsync(): Ocurrió un error al intentar obtener Detalle de Aplicaciones de Oracle", _servicios.ReportesOracle);
                    Log.Error("No se pudo obtener el Detalle de Aplicaciones.\n Cuit:{cuit}\n Request: {@request} \n Response: {@response}", userCuit, request, paymentsResponse);
                }
                else
                {
                    Monitoreo.Monitor.Ok("GetApplicationDetailProductListAsync(): Se obtuvo Detalle de Aplicaciones de Oracle", _servicios.ReportesOracle);
                }

                var result = new List<CuitProductBuDto>();

                if (paymentsResponse.Data != null)
                {
                    result = paymentsResponse.Data.GroupBy(l => new { l.CustomerDocumentNumber, l.Producto, l.BusinessUnitName, l.ReferenciaCliente }).Select(cl =>
                        new CuitProductBuDto()
                        {
                            Cuit = cl.Key.CustomerDocumentNumber,
                            Product = cl.Key.Producto,
                            BusinessUnit = cl.Key.BusinessUnitName,
                            ReferenciaCliente = cl.Key.ReferenciaCliente
                        }).ToList();
                }

                return result;
            }
            catch (Exception ex)
            {
                Monitoreo.Monitor.Critical("GetApplicationDetailProductListAsync(): Error al obtener los detalles de aplicaciones del Sgf", _servicios.ReportesOracle);

                Log.Error(ex, @"GetApplicationDetailProductListAsync, 
                    Type:GetApplicationDetailProductListAsyncError,
                    Description: Error fetching ApplicationDetailProductList:
                    request: {@request}", request);
                throw;
            }
        }

        public List<CuitProductCurrencyDto> GetApplicationDetailProductList(List<string> userCuits)
        {
            var requestModel = new ResumenCuentaRequest();
            requestModel.TipoDocumento = "CUIT";
            requestModel.NroDocumentos = userCuits;

            _restClient.BaseUrl = new Uri(_apiServicesConfig.Get(ApiServicesConfig.SgfApi).Url);
            RestRequest request = new RestRequest("/Deuda/ObtenerDetalleAplicaciones", Method.POST);
            request.AddHeader("Token", _apiServicesConfig.Get(ApiServicesConfig.SgfApi).Token);
            request.AddJsonBody(requestModel);
            try
            {

                IRestResponse<List<ApplicationDetailDto>> paymentsResponse = _restClient.Execute<List<ApplicationDetailDto>>(request);
                if (!paymentsResponse.IsSuccessful)
                {
                    Monitoreo.Monitor.Critical("GetApplicationDetailProductList(): Ocurrió un error al intentar obtener Detalle de Aplicaciones de Oracle", _servicios.ReportesOracle);
                    Log.Error("No se pudo obtener el Detalle de Aplicaciones.\n Cuit:{cuit}\n Request: {@request} \n Response: {@response}", userCuits, request, paymentsResponse);
                }
                else
                {
                    Monitoreo.Monitor.Ok("GetApplicationDetailProductList(): Se obtuvo Detalle de Aplicaciones de Oracle", _servicios.ReportesOracle);
                }

                var result = new List<CuitProductCurrencyDto>();

                if (paymentsResponse.Data != null)
                {
                    result = paymentsResponse.Data.GroupBy(l => new { l.CustomerDocumentNumber, l.Producto }).Select(cl =>
                        new CuitProductCurrencyDto()
                        {
                            Cuit = cl.Key.CustomerDocumentNumber,
                            Product = cl.Key.Producto
                        }).ToList();
                }

                return result;
            }
            catch (Exception ex)
            {
                Monitoreo.Monitor.Critical("GetApplicationDetailProductList(): Error al obtener los detalles de aplicaciones del Sgf", _servicios.ReportesOracle);

                Log.Error(ex, @"GetApplicationDetailProductList, 
                    Type:GetApplicationDetailProductListError,
                    Description: Error fetching ApplicationDetailProductList:
                    request: {@request}", request);
                throw;
            }
        }

        public List<CuitProductCurrencyDto> GetDetailAndBalanceProductList(bool canViewAll, List<string> userCuits, User user = null)
        {
            var result = new List<CuitProductCurrencyDto>();
            var cachedPropCodes = _distributedCache.Get(_propertyCodesListKey);
            if (cachedPropCodes != null)
            {
                var bytesAsString = Encoding.UTF8.GetString(cachedPropCodes);
                result = JsonConvert.DeserializeObject<List<CuitProductCurrencyDto>>(bytesAsString);
            }
            else
            {
                result = AsyncHelper.RunSync(() => FetchDetailAndBalanceProductList());
            }
            if (!canViewAll)
            {
                
                    result = user.IsForeignCuit 
                        ? result.Where(x => userCuits.Contains(x.Cuit) && user.ClientReference == x.ReferenciaCliente).ToList()
                        : result.Where(x => userCuits.Contains(x.Cuit)).ToList();
            }

            return result.DistinctBy(x => new { x.Cuit, x.Product }).ToList();
        }

        [Queue("cache")]
        [DisableConcurrentExecution(timeoutInSeconds: 1800)]
        public async Task<List<CuitProductCurrencyDto>> FetchDetailAndBalanceProductList()
        {
            try
            {
                var result = new List<CuitProductCurrencyDto>();

                // tasks
                var taskApplicationDetail = this.GetApplicationDetailProductListAsync(string.Empty);
                var taskBalanceDetail = this.GetBalanceDetailProductListAsync(string.Empty);
                var taskUnappliedProduct = this.GetUnappliedProductListAsync(string.Empty);

                // wait all
                Task.WaitAll(taskApplicationDetail, taskBalanceDetail, taskUnappliedProduct);

                // handle response of each task
                var applicationProducts = taskApplicationDetail.Result.Select(x => new CuitProductCurrencyDto() { Cuit = x.Cuit, Product = x.Product, ReferenciaCliente = x.ReferenciaCliente });
                var balanceProducts = taskBalanceDetail.Result.Select(x => new CuitProductCurrencyDto() { Cuit = x.Cuit, Product = x.Product, ReferenciaCliente = x.ReferenciaCliente });
                var unappliedProducts = taskUnappliedProduct.Result.Select(x => new CuitProductCurrencyDto() { Cuit = x.Cuit, Product = x.Product, ReferenciaCliente = x.ReferenciaCliente });

                // join products
                result = applicationProducts
                    .Union(balanceProducts, new CuitProductComparer())
                    .Union(unappliedProducts, new CuitProductComparer())
                    .ToList();

                // save to cache
                if (result.Any())
                {
                    var serializeObject = JsonConvert.SerializeObject(result);
                    byte[] cachedPropCodes = Encoding.UTF8.GetBytes(serializeObject);
                    await _distributedCache.SetAsync(_propertyCodesListKey, cachedPropCodes);
                }

                return result;
            }
            catch (Exception ex)
            {
                Log.Error(ex, @"FetchDetailAndBalanceProductList, 
                    Type:FetchDetailAndBalanceProductListError,
                    Description: Error fetching DetailAndBalanceProductList");
                throw;
            }
        }

        public List<CuitProductCurrencyDto> GetDetailAndBalanceProductListAsync(string userCuit)
        {
            // tasks
            var taskApplicationDetail = this.GetApplicationDetailProductListAsync(userCuit);
            var taskBalanceDetail = this.GetBalanceDetailProductListAsync(userCuit);
            var taskUnappliedProduct = this.GetUnappliedProductListAsync(userCuit);

            // wait all
            Task.WaitAll(taskApplicationDetail, taskBalanceDetail, taskUnappliedProduct);

            // handle response of each task
            var applicationProducts = taskApplicationDetail.Result.Select(x => new CuitProductCurrencyDto() { Cuit = x.Cuit, Product = x.Product });
            var balanceProducts = taskBalanceDetail.Result.Select(x => new CuitProductCurrencyDto() { Cuit = x.Cuit, Product = x.Product });
            var unappliedProducts = taskUnappliedProduct.Result.Select(x => new CuitProductCurrencyDto() { Cuit = x.Cuit, Product = x.Product });

            // join products
            var result = applicationProducts
                .Union(balanceProducts, new CuitProductComparer())
                .Union(unappliedProducts, new CuitProductComparer())
                .ToList();

            return result;
        }

        public List<CuitProductCurrencyDto> GetDetailAndBalanceProductListExternal(User user)
        {
            var applicationProducts = this.GetApplicationDetailProductList(string.Empty)
                .Where(x => user.BusinessUnits.Any(y => y.Name.ToLower().Equals(x.BusinessUnit.ToLower())))
                .Select(x => new CuitProductCurrencyDto() { Cuit = x.Cuit, Product = x.Product });
            var balanceProducts = this.GetBalanceDetailProductList(string.Empty)
                .Where(x => user.BusinessUnits.Any(y => y.Name.ToLower().Equals(x.BusinessUnit.ToLower())))
                .Select(x => new CuitProductCurrencyDto() { Cuit = x.Cuit, Product = x.Product });
            var unappliedProducts = this.GetUnappliedProductList(string.Empty)
                .Where(x => user.BusinessUnits.Any(y => y.Name.ToLower().Equals(x.BusinessUnit.ToLower())))
                .Select(x => new CuitProductCurrencyDto() { Cuit = x.Cuit, Product = x.Product });

            var result = applicationProducts
                .Union(balanceProducts, new CuitProductComparer())
                .Union(unappliedProducts, new CuitProductComparer())
                .ToList();

            return result;
        }

        public List<SalesInvoiceAmountDto> GetSalesInvoiceAmount(List<PropertyCodeCuitDto> propertyCodes)
        {
            List<SalesInvoiceAmountDto> salesInvoices = new List<SalesInvoiceAmountDto>();
            _restClient.BaseUrl = new Uri(_apiServicesConfig.Get(ApiServicesConfig.SgcApi).Url);
            RestRequest request = new RestRequest("/Boleto/Monto", Method.POST)
            {
                RequestFormat = DataFormat.Json
            };
            request.AddHeader("token", _apiServicesConfig.Get(ApiServicesConfig.SgcApi).Token);
            request.AddJsonBody(propertyCodes);

            try
            {

                IRestResponse<List<SalesInvoiceAmountDto>> response = AsyncHelper.RunSync(
                async () => await _restClient.ExecuteAsync<List<SalesInvoiceAmountDto>>(request));

                if (!response.IsSuccessful)
                {
                    Log.Error("No se pudo obtener Sales Invoice Amount.\n Request: {@request} \n Response: {@response}", request, response);
                }

                if (response.Data != null)
                {
                    salesInvoices = response.Data;
                }

                return salesInvoices;
            }
            catch (Exception ex)
            {
                Log.Error(ex, @"GetSalesInvoiceAmount, 
                    Type:GetSalesInvoiceAmountError,
                    Description: Error fetching SalesInvoiceAmount:
                    request: {@request}", request);
                throw;
            }
        }

        public async Task<List<BalanceDetailDto>> GetBalanceDetailAsync(ResumenCuentaRequest requestModel = null)
        {
            if (!requestModel.NroDocumentos.Any()) return new List<BalanceDetailDto>();

            requestModel ??= new ResumenCuentaRequest();
            _restClient.BaseUrl = new Uri(_apiServicesConfig.Get(ApiServicesConfig.SgfApi).Url);
            var request = new RestRequest("/Deuda/ObtenerDetalleSaldos", Method.POST);
            request.AddHeader("Token", _apiServicesConfig.Get(ApiServicesConfig.SgfApi).Token);
            request.AddJsonBody(requestModel);

            try
            {
                var response = await _restClient.ExecuteAsync<List<BalanceDetailDto>>(request);
                if (response.IsSuccessful)
                {
                    Monitoreo.Monitor.Ok("GetBalanceDetailAsync(): Se obtuvo Detalle de Balances de Oracle", _servicios.ReportesOracle);
                    Log.Information("GetBalanceDetailAsync to SGF ," +
                        "request: {@request} ," +
                        "response:{@response} ", request, response);

                    return response.Data;
                }
                else
                {
                    Monitoreo.Monitor.Critical("GetBalanceDetailAsync(): Ocurrió un error al intentar obtener Detalle de Balances de Oracle", _servicios.ReportesOracle);
                    Log.Warning(@"GetBalanceDetailAsync,
                        Type:GetBalanceDetailAsyncWarning,
                        Description: {msg}:
                        request: {@request},
                        response: {@response}", response.ErrorMessage, request, response);
                    return new List<BalanceDetailDto>();
                }
            }
            catch (Exception ex)
            {
                Monitoreo.Monitor.Critical("GetBalanceDetailAsync(): Error al obtener los detalles de saldos del Sgf", _servicios.ReportesOracle);
                Log.Error(ex, @"GetBalanceDetailAsync, 
                    TypeGetBalanceDetailAsyncError,
                    Description: {msg}
                    request: {@request}", ex.Message, request);
                throw;
            }
        }

        public List<ProductCodeBusinessUnitDTO> GetBusinessUnitByProductCodes(List<string> productCodes)
        {
            List<ProductCodeBusinessUnitDTO> businessUnits = new List<ProductCodeBusinessUnitDTO>();
            _restClient.BaseUrl = new Uri(_apiServicesConfig.Get(ApiServicesConfig.SgcApi).Url);
            RestRequest request = new RestRequest("/Producto/BusinessUnit", Method.POST)
            {
                RequestFormat = DataFormat.Json
            };
            request.AddHeader("token", _apiServicesConfig.Get(ApiServicesConfig.SgcApi).Token);
            request.AddJsonBody(productCodes);

            Serilog.Log.Information("request: {@request}", request);

            IRestResponse<List<ProductCodeBusinessUnitDTO>> response = AsyncHelper.RunSync(
                async () => await _restClient.ExecuteAsync<List<ProductCodeBusinessUnitDTO>>(request));

            if (response.IsSuccessful)
            {
                businessUnits = response.Data;
            }

            return businessUnits;
        }

        public async Task<List<ApplicationDetailDto>> GetApplicationDetailAsync(ResumenCuentaRequest requestModel = null)
        {
            if (!requestModel.NroDocumentos.Any()) return new List<ApplicationDetailDto>();

            requestModel ??= new ResumenCuentaRequest();
            _restClient.BaseUrl = new Uri(_apiServicesConfig.Get(ApiServicesConfig.SgfApi).Url);
            var request = new RestRequest("/Deuda/ObtenerDetalleAplicaciones", Method.POST);
            request.AddHeader("Token", _apiServicesConfig.Get(ApiServicesConfig.SgfApi).Token);
            request.AddJsonBody(requestModel);

            try
            {
                var response = await _restClient.ExecuteAsync<List<ApplicationDetailDto>>(request);
                if (response.IsSuccessful)
                {
                    Monitoreo.Monitor.Ok("GetApplicationDetail(): Se obtuvo Detalle de Aplicaciones de Oracle", _servicios.ReportesOracle);
                    Log.Information("GetApplicationDetailAsync to SGF ," +
                        "request: {@request} ," +
                        "response:{@response} ", request, response);

                    return response.Data;
                }
                else
                {
                    Monitoreo.Monitor.Critical("GetApplicationDetail(): Ocurrió un error al intentar obtener Detalle de Aplicaciones de Oracle", _servicios.ReportesOracle);
                    Log.Warning(@"GetApplicationDetailAsync,
                        Type:GetApplicationDetailAsyncWarning,
                        Description: {msg}
                        request: {@request},
                        response: {@response}", response.ErrorMessage, request, response);
                    return new List<ApplicationDetailDto>();
                }
            }
            catch (Exception ex)
            {
                Monitoreo.Monitor.Critical("GetApplicationDetail(): Error al obtener los detalles de aplicaciones del Sgf", _servicios.ReportesOracle);
                Log.Error(ex, @"GetApplicationDetailAsync, 
                    Type:GetApplicationDetailAsyncError,
                    Description: {msg}
                    request: {@request}", ex.Message, request);
                throw;
            }
        }

        private string NormalizeString(string stringInput)
        {
            byte[] tempBytes;
            tempBytes = System.Text.Encoding.GetEncoding("ISO-8859-8").GetBytes(stringInput);
            string asciiStr = System.Text.Encoding.UTF8.GetString(tempBytes);

            return asciiStr;
        }

        private string GetProductCode(string producto)
        {
            string[] arrCode = producto.Split(" ");
            string nombreEmprendimiento = arrCode[0].Trim();
            string nroLote = arrCode.Last().Trim();
            _restClient.BaseUrl = new Uri(_apiServicesConfig.Get(ApiServicesConfig.SgcApi).Url);
            RestRequest request = new RestRequest("/Producto/Productos/" + nombreEmprendimiento + "/" + nroLote, Method.GET);
            request.AddHeader("Token", _apiServicesConfig.Get(ApiServicesConfig.SgcApi).Token);

            IRestResponse<dynamic> productResponse = _restClient.Execute<dynamic>(request);

            Log.Debug("Se trae CodigoProducto desde SGC. \n Producto: {producto} \n RequestUrl: {@request} \n ResponseData: {@response}", producto, request.Resource, productResponse.Data);

            if (!productResponse.IsSuccessful)
            {
                Log.Error("No se pudo obtener el Codigo de Producto desde SGC. \n Producto: {producto} \n Request: {@request} \n Response: {@response}", producto, request, productResponse);
                return null;
            }

            return productResponse.Data?["codigo"];
        }

        public void InformPaymentDone(IOrderedEnumerable<DetalleDeuda> debtsWithSameCurrencyThanDebin, DateTime? fechaRecibo = null)
        {
            Log.Debug(@"Starting InformPaymentDone to SGF");
            List<object> payments = new List<object>();
            var paymentMethod = debtsWithSameCurrencyThanDebin.First().PaymentMethod;
            double totalAmount = paymentMethod.Amount;

            foreach (DetalleDeuda detalleDeuda in debtsWithSameCurrencyThanDebin)
            {
                DetalleDeuda deuda = _archivoDeudaRepository.Find(detalleDeuda.Id);

                double amount = CalcDebtPaidAmount(deuda, ref totalAmount);

                var codigoAcuerdo = paymentMethod.Source == PaymentSource.Itau ? deuda.ArchivoDeuda.Header.CodOrganismo : paymentMethod.OlapAcuerdo;

                var paymentInfo = new
                {
                    codigoAcuerdo = Convert.ToInt64(codigoAcuerdo),
                    moneda = deuda.CodigoMoneda,
                    monto = amount.ToString("#.##", CultureInfo.InvariantCulture),
                    cotizacion = Convert.ToDouble(deuda.ObsLibreTercera, CultureInfo.InvariantCulture).ToString(CultureInfo.InvariantCulture).Replace(",", "."),
                    transaction = deuda.ObsLibreSegunda.Split('|')[0],
                    fechaVencimiento = DateTime.ParseExact(deuda.FechaPrimerVenc, "yyyyMMdd", CultureInfo.InvariantCulture).ToString("yyyy-MM-dd"),//year + "-" + month + "-" + day,
                    idClienteOracle = Convert.ToInt32(deuda.DescripcionLocalidad),
                    idSiteClienteOracle = Convert.ToInt32(deuda.DireccionCliente),
                    metodo = paymentMethod.OlapMethod,
                    fechaRecibo,
                    paymentMethodId = paymentMethod.Id
                };

                payments.Add(paymentInfo);
            }

            _restClient.BaseUrl = new Uri(_apiServicesConfig.Get(ApiServicesConfig.SgfApi).Url);
            RestRequest request = new RestRequest("/Deuda/InformarPago", Method.POST)
            {
                RequestFormat = DataFormat.Json
            };
            request.AddHeader("token", _apiServicesConfig.Get(ApiServicesConfig.SgfApi).Token);
            request.AddJsonBody(payments);

            try
            {
                IRestResponse<List<string>> response = _restClient.Execute<List<string>>(request);
                if (response.IsSuccessful)
                {
                    Log.Information("InformPaymentDone a {url} del {className}: {@paymentMethod}", request.Resource,
                        paymentMethod.GetType().Name, paymentMethod);
                    Log.Debug(
                        "InformPaymentDone to SGF \n {className}: {@paymentMethod} \n request: {@request} \n response:{@response}",
                        paymentMethod.GetType().Name, paymentMethod, request, response);
                }
                else
                {
                    Log.Warning(@"InformPaymentDone, 
                        Type:InformPaymentDoneWarning,
                        Description: Response not successful:
                        request: {@request},
                        response: {@response}", request, response);
                }
            }
            catch (Exception ex)
            {
                Log.Error(ex, @"InformPaymentDone, 
                    Type:InformPaymentDoneError,
                    Description: Error when InformPaymentDone to SGF:
                    request: {@request}", request);
            }
        }

        public void InformPaymentMethodDone(PaymentMethod paymentMethod, int idClientOracle, int idSiteClientOracle, DateTime? fechaRecibo = null)
        {
            Log.Debug(@"Starting InformPaymentMethodDone to SGF");

            var receiptDate = (DateTime)fechaRecibo;
            var paymentMethods = new List<dynamic>();
            var quotation = (DolarMEP)_exchangeRateFileRepository.GetQuotationBetweenDate("DolarMEP", (DateTime)fechaRecibo);

            if ((double)quotation.Valor == 0)
            {
                throw new Exception("No se encontro cotizacion DolarMep para la fecha {fechaRecibo}");
            }

            var paymentInfo = new
            {
                moneda = paymentMethod.Currency.ToString(),
                monto = paymentMethod.Amount.ToString("#.##", CultureInfo.InvariantCulture),
                cotizacion = paymentMethod.Currency == Currency.ARS ? ((double)quotation.Valor).ToString(CultureInfo.InvariantCulture).Replace(",", ".") : 
                             paymentMethod.Currency == Currency.USD ? "1" : "1",       
                transaction = string.Empty,
                fechaVencimiento = paymentMethod.TransactionDate, 
                idClienteOracle = idClientOracle,
                idSiteClienteOracle = idSiteClientOracle,
                codigoAcuerdo = Convert.ToInt64(paymentMethod.OlapAcuerdo),
                metodo = paymentMethod.OlapMethod,
                fechaRecibo,
                paymentMethodId = paymentMethod.Id
            };

            paymentMethods.Add(paymentInfo);

            _restClient.BaseUrl = new Uri(_apiServicesConfig.Get(ApiServicesConfig.SgfApi).Url);
            var request = new RestRequest("/Deuda/InformarPago", Method.POST)
            {
                RequestFormat = DataFormat.Json,
                
            };

            var options = new JsonSerializerSettings
            {
                DateTimeZoneHandling = DateTimeZoneHandling.RoundtripKind
            };

            var payload = JsonConvert.SerializeObject(paymentMethods, options);

            request.AddHeader("token", _apiServicesConfig.Get(ApiServicesConfig.SgfApi).Token);
            //request.AddJsonBody(paymentMethods);
            request.AddParameter("application/json", payload, ParameterType.RequestBody);

            try
            {
                IRestResponse<List<string>> response = AsyncHelper.RunSync(
                    async () => await _restClient.ExecuteAsync<List<string>>(request));
                if (response.IsSuccessful)
                {
                    Log.Information("InformPaymentMethodDone a {url} del {className}: {@paymentMethod}", request.Resource,
                        paymentMethod.GetType().Name, paymentMethod);
                    Log.Debug(
                        "InformPaymentMethodDone to SGF \n {className}: {@paymentMethod} \n request: {@request} \n response:{@response}",
                        paymentMethod.GetType().Name, paymentMethod, request, response);
                }
                else
                {
                    Log.Warning(@"InformPaymentMethodDone, 
                        Type:InformPaymentMethodDoneWarning,
                        Description: Response not successful:
                        request: {@request},
                        response: {@response}", request, response);
                }
            }
            catch (Exception e)
            {
                Log.Error(@"InformPaymentMethodDone, 
                    Type:InformPaymentMethodDoneError,
                    Description: Error when InformPaymentMethodDone to SGF:
                    request: {@request},
                    response: {@error}", request, e);
            }
        }

        private double CalcDebtPaidAmount(DetalleDeuda deuda, ref double debinAmount)
        {
            if (deuda.PaymentMethod != null)
            {
                double totalAmount = debinAmount;
                double aDebtAmount = Convert.ToDouble(deuda.ImportePrimerVenc) / 100;

                if (totalAmount >= aDebtAmount)
                {
                    debinAmount = totalAmount - aDebtAmount;
                    return aDebtAmount;
                }
                return totalAmount;
            }
            else
            {
                Log.Warning("CalcDebtAmount fallo, devolviendo 0 porque la deuda tiene debin null. \n DetalleDeuda: {@deuda} \n refDebinAmount: {debinAmount}", deuda, debinAmount);
                return 0;
            }
        }

        public List<DetalleDeuda> GetAllPayments(string cuit = "", string accountNumber = "")
        {
            return _archivoDeudaRepository.All(cuit, accountNumber);
        }

        public List<DetalleDeuda> GetAllPayments(List<string> cuits)
        {
            return _archivoDeudaRepository.All(cuits);
        }

        public List<DetalleDeuda> GetPaymentsByFFileName(string Cuit, string FFileName)
        {
            return _archivoDeudaRepository.GetByFFileName(Cuit, FFileName);
        }

        public List<PropertyCode> GetPropertyCodes(List<string> cuits = null, string accountNumber = null)
        {
            var propertyCodes = _archivoDeudaRepository.GetPropertyCodes(cuits, accountNumber);

            GetEmprendimientos(propertyCodes);
            GetBusinessUnits(propertyCodes);
            GetRazonSocial(propertyCodes);

            return propertyCodes;
        }

        private void GetBusinessUnits(List<PropertyCode> propertyCodes)
        {
            var productCodes = propertyCodes.Where(x => !string.IsNullOrEmpty(x.ProductCode)).Select(x => x.ProductCode).ToList();
            var productBusinessUnits = GetBusinessUnitByProductCodes(productCodes);
            propertyCodes.ForEach(x =>
                        x.BusinessUnit = productBusinessUnits.FirstOrDefault(pbu => pbu.Codigo.Equals(x.ProductCode))
                            ?.BusinessUnit);
        }
        private IEnumerable<string> GetBusinessUnitsForPropertyCodes(IEnumerable<string> propertyCodes)
        {
            var productCodes = propertyCodes.Where(x => !string.IsNullOrEmpty(x)).ToList();
            var productBusinessUnits = GetBusinessUnitByProductCodes(productCodes);
            return productBusinessUnits.Select(x => x.BusinessUnit);
        }

        private void GetEmprendimientos(List<PropertyCode> propertyCodes, User user = null)
        {
            Log.Debug(@"Starting GetEmprendimientos for PropertyCodes");
            // CASO 2 (cobra-api): puede ocurrir que haya códigos de producto nulos o vacíos. Como no se mandan a sgc-api, no reciben nombre de emprendimiento
            IQueryable<PropertyCode> productCodesQuery = propertyCodes.AsQueryable()
                .Where(x => !string.IsNullOrEmpty(x.ProductCode));
            if (user is { IsForeignCuit: true })
            {
                productCodesQuery = productCodesQuery.
                    Where(x => x.ReferenciaCliente == user.ClientReference);
            }
            string[] productCodes = productCodesQuery
                .Select(x => x.ProductCode)
                .ToArray();

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
                    Log.Debug(@"GetEmprendimientos, 
                        Type: GetEmprendimientosSuccess,
                        Description: Response successful for PropertyCodes:
                        request: {@request},
                        response: {@response}", request, response);

                    
                    propertyCodes.ForEach(x =>
                        x.Emprendimiento = response.Data.Any(y => y.codigo.Equals(x.ProductCode))
                            ? response.Data.FirstOrDefault(
                                    y => y.codigo.Equals(x.ProductCode)
                                         && (user is not { IsForeignCuit: true } || x.ReferenciaCliente == user.ClientReference))
                                ?.emprendimiento
                            : "SIN CÓDIGO DE PRODUCTO"
                    );

                }
                else
                {
                    Log.Information(@"GetEmprendimientos, 
                        Type:GetEmprendimientosWarning,
                        Description: Response not successful or Data is null:
                        request: {@request},
                        response: {@response}", request, response);
                }
            }
            catch (Exception ex)
            {
                Log.Error(ex, @"GetEmprendimientos, 
                    Type:GetEmprendimientosError,
                    Description: Error fetching Emprendimientos for PropertyCodes:
                    request: {@request}", request);
                throw;
            }
        }
        private void GetEmprendimientos(List<PropertyCodeFull> propertyCodes)
        {
            Log.Debug(@"Starting GetEmprendimientos for PropertyCodesFull");
            var productCodes = propertyCodes.Where(x => !string.IsNullOrEmpty(x.ProductCode)).Select(x => x.ProductCode)
                .ToArray();
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
                    Log.Debug(@"GetEmprendimientos, 
                        Type: GetEmprendimientosSuccess,
                        Description: Response successful for PropertyCodesFull:
                        request: {@request},
                        response: {@response}", request, response);
                    propertyCodes.ForEach(x =>
                        x.Emprendimiento = response.Data.FirstOrDefault(y => y.codigo.Equals(x.ProductCode))?.emprendimiento);
                }
                else
                {
                    Log.Information(@"GetEmprendimientos, 
                        Type:GetEmprendimientosWarning,
                        Description: Response not successful or Data is null:
                        request: {@request},
                        response: {@response}", request, response);
                }
            }
            catch (Exception ex)
            {
                Log.Error(ex, @"GetEmprendimientos, 
                    Type:GetEmprendimientosError,
                    Description: Error fetching Emprendimientos:
                    request: {@request}", request);
                throw;
            }
        }

        public List<PropertyCode> GetPropertyCodesForSummaryExternal(User user)
        {
            var result = this.GetDetailAndBalanceProductListExternal(user).Select(x => new PropertyCode()
            {
                ProductCode = x.Product,
                NroCuitCliente = x.Cuit
            }).ToList();

            if (user.IsForeignCuit)
            {
                GetEmprendimientos(result, user);

            }
            else
            {
                GetEmprendimientos(result);
            }

            return result;
        }
        public List<PropertyCode> GetPropertyCodesForSummary(bool canViewAll, List<string> clientCuits, User user)
        {

            var result = this.GetDetailAndBalanceProductList(canViewAll, clientCuits, user).Select(x => new PropertyCode()
            {
                ProductCode = x.Product,
                NroCuitCliente = x.Cuit,
                ReferenciaCliente = user.IsForeignCuit ? x.ReferenciaCliente : null
            }).ToList();

            if (user.IsForeignCuit)
            {
                GetEmprendimientos(result, user);
            }
            else
            {
                GetEmprendimientos(result);

            }

            GetRazonSocial(result);

            return result;
        }

        public List<PropertyCode> GetPropertyCodesForAdvance(bool canViewAll, List<string> clientCuits)
        {
            var result = new List<PropertyCode>();
            var cachedPropCodes = _distributedCache.Get(_propertyCodesListForAdvance);
            if (cachedPropCodes != null)
            {
                var bytesAsString = Encoding.UTF8.GetString(cachedPropCodes);
                result = JsonConvert.DeserializeObject<List<PropertyCode>>(bytesAsString);
            }
            else
            {
                var balanceDetails = this.GetBalanceDetailProductList(string.Empty);
                // save to cache
                if (balanceDetails.Any())
                {
                    // exclude BU Huergo
                    balanceDetails = balanceDetails.Where(x => !BusinessUnitsDisabledForAdvancePayments.Any(y => y == x.BusinessUnit)).ToList();
                    result = balanceDetails.Select(x => new PropertyCode()
                    {
                        BusinessUnit = x.BusinessUnit,
                        ProductCode = x.Product,
                        NroCuitCliente = x.Cuit
                    }).ToList();
                    var serializeObject = JsonConvert.SerializeObject(result);
                    byte[] cachedPropCodesToSave = Encoding.UTF8.GetBytes(serializeObject);
                    _distributedCache.Set(_propertyCodesListForAdvance, cachedPropCodesToSave);
                }
            }
            if (!canViewAll)
            {
                result = result.Where(x => clientCuits.Contains(x.NroCuitCliente)).ToList();
            }

            GetEmprendimientos(result);
            GetRazonSocial(result);

            return result;
        }

        private void GetRazonSocial(List<PropertyCode> result)
        {
            var userCuits = result.Select(x => x.NroCuitCliente).Distinct().ToList();
            var users = _userRepository.GetUsersByCuits(userCuits);
            result.ForEach(
                x => x.RazonSocial = users.FirstOrDefault(
                        y => y.Cuit == x.NroCuitCliente
                        && (string.IsNullOrEmpty(x.AccountNumber) || y.AccountNumber == x.AccountNumber))
                ?.RazonSocial
            );

            List<string> cuitsNoNames = result.Where(x => string.IsNullOrEmpty(x.RazonSocial)).Select(x => x.NroCuitCliente).Distinct().ToList();
            List<ContactDetailDto> names = new List<ContactDetailDto>();
            int maxBatch = 999;
            for (int i = 0; i <= cuitsNoNames.Count(); i += maxBatch)
            {
                var cuits = cuitsNoNames.Skip(i).Take(maxBatch).ToList();
                names.AddRange(_contactDetailService.GetClienteDatosContactos(cuits));
            }

            result.ForEach(x =>
            {
                if (string.IsNullOrEmpty(x.RazonSocial))
                    x.RazonSocial = names.FirstOrDefault(y => x.NroCuitCliente == y.DocumentNumber)?.PartyName;
            });
        }

        public List<PropertyCodeFull> GetPropertyCodesFull()
        {
            var propertyCodes = _archivoDeudaRepository.GetPropertyCodesFull();
            GetEmprendimientos(propertyCodes);
            return propertyCodes;
        }
        public Dictionary<string, string> GetBusinessUnitByProductCodeDictionary(List<string> productCodes)
        {
            Dictionary<string, string> productCodeByBuDict = new Dictionary<string, string>();
            var buList = GetBusinessUnitByProductCodes(productCodes).DistinctBy(x => new { x.Codigo, x.BusinessUnit }).ToList();
            buList.ForEach(x =>
            {
                productCodeByBuDict.Add(x.Codigo, x.BusinessUnit);
            });
            return productCodeByBuDict;
        }

        public string GetReceipt(string buId, string docId, string legalEntityId)
        {
            _restClient.BaseUrl = new Uri(_apiServicesConfig.Get(ApiServicesConfig.SgfApi).Url);
            RestRequest request = new RestRequest("/Deuda/ObtenerRecibo", Method.GET);
            request.AddHeader("Token", _apiServicesConfig.Get(ApiServicesConfig.SgfApi).Token);
            request.AddParameter("buId", buId);
            request.AddParameter("docId", docId);
            request.AddParameter("legalEntityId", legalEntityId);

            IRestResponse<dynamic> paymentsResponse = _restClient.Execute<dynamic>(request);
            if (!paymentsResponse.IsSuccessful)
            {
                Log.Error("No se pudo obtener Recibo.\n buId:{buId} \n docId: {docId} \n legalEntityId: {legalEntityId} \n Request: {@request} \n Response: {@response}", buId, docId, legalEntityId, request, paymentsResponse);
            }

            var response = paymentsResponse.Data;

            return response;
        }

        public List<UnappliedPaymentDto> GetUnappliedPayments(string cuit, string productCode)
        {
            var requestModel = new ResumenCuentaRequest();
            if (!string.IsNullOrEmpty(cuit))
            {
                requestModel.TipoDocumento = "CUIT";
                requestModel.NroDocumentos.Add(cuit);
            }

            _restClient.BaseUrl = new Uri(_apiServicesConfig.Get(ApiServicesConfig.SgfApi).Url);
            RestRequest request = new RestRequest("/Deuda/ObtenerCobrosNoAplicados", Method.POST);
            request.AddHeader("Token", _apiServicesConfig.Get(ApiServicesConfig.SgfApi).Token);
            request.AddJsonBody(requestModel);

            IRestResponse<List<DetailedUnappliedPayment>> paymentsResponse = _restClient.Execute<List<DetailedUnappliedPayment>>(request);
            if (!paymentsResponse.IsSuccessful)
            {
                Monitoreo.Monitor.Critical("GetUnappliedPayments(): Ocurrió un error al intentar obtener Cobros No Aplicados de Oracle", _servicios.ReportesOracle);
                Log.Error("No se pudo obtener Cobros No Aplicados.\n Cuit:{cuit} \n CodProducto: {productCode} \n Request: {@request} \n Response: {@response}", cuit, productCode, request, paymentsResponse);
            }
            else
            {
                Monitoreo.Monitor.Ok("GetUnappliedPayments(): Se obtuvo Cobros No Aplicados de Oracle", _servicios.ReportesOracle);
            }

            List<DetailedUnappliedPayment> response = paymentsResponse.Data;

            List<DetailedUnappliedPayment> filteredResponse = response.Where(x => x.Producto == productCode).ToList();


            List<UnappliedPaymentDto> result = new List<UnappliedPaymentDto>();

            if (filteredResponse.Any())
            {
                result = filteredResponse.Select(x => new UnappliedPaymentDto()
                {
                    Fecha = x.DocDate,
                    Moneda = x.CurrencyCode,
                    Importe = x.Amount,
                    ImporteTc = x.PddRate,
                    Operacion = x.DocType,
                    Conversion = string.IsNullOrEmpty(x.PddRate) ? string.Empty : Math.Round((x.Amount.GetDouble() / x.PddRate.GetDouble()), 2).ToString(new CultureInfo("es-AR"))
                }).ToList();
            }

            return result;
        }

        public List<UnappliedPaymentDto> GetUnappliedPayments(string cuit, string productCode, string clientReference = "")
        {
            var requestModel = new ResumenCuentaRequest();
            if (!string.IsNullOrEmpty(cuit))
            {
                requestModel.TipoDocumento = "CUIT";
                requestModel.NroDocumentos.Add(cuit);
            }

            _restClient.BaseUrl = new Uri(_apiServicesConfig.Get(ApiServicesConfig.SgfApi).Url);
            RestRequest request = new RestRequest("/Deuda/ObtenerCobrosNoAplicados", Method.POST);
            request.AddHeader("Token", _apiServicesConfig.Get(ApiServicesConfig.SgfApi).Token);
            request.AddJsonBody(requestModel);

            IRestResponse<List<DetailedUnappliedPayment>> paymentsResponse = _restClient.Execute<List<DetailedUnappliedPayment>>(request);
            if (!paymentsResponse.IsSuccessful)
            {
                Monitoreo.Monitor.Critical("GetUnappliedPayments(): Ocurrió un error al intentar obtener Cobros No Aplicados de Oracle", _servicios.ReportesOracle);
                Log.Error("No se pudo obtener Cobros No Aplicados.\n Cuit:{cuit} \n CodProducto: {productCode} \n Request: {@request} \n Response: {@response}", cuit, productCode, request, paymentsResponse);
            }
            else
            {
                Monitoreo.Monitor.Ok("GetUnappliedPayments(): Se obtuvo Cobros No Aplicados de Oracle", _servicios.ReportesOracle);
            }

            List<DetailedUnappliedPayment> response = paymentsResponse.Data;

            List<DetailedUnappliedPayment> filteredResponse = response.Where(x => x.Producto == productCode && x.ReferenciaCliente == clientReference).ToList();


            List<UnappliedPaymentDto> result = new List<UnappliedPaymentDto>();

            if (filteredResponse.Any())
            {
                result = filteredResponse.Select(x => new UnappliedPaymentDto()
                {
                    Fecha = x.DocDate,
                    Moneda = x.CurrencyCode,
                    Importe = x.Amount,
                    ImporteTc = x.PddRate,
                    Operacion = x.DocType,
                    Conversion = string.IsNullOrEmpty(x.PddRate) ? string.Empty : Math.Round((x.Amount.GetDouble() / x.PddRate.GetDouble()), 2).ToString(new CultureInfo("es-AR"))
                }).ToList();
            }

            return result;
        }


        public string GetInvoice(string trxId, string facElect)
        {
            _restClient.BaseUrl = new Uri(_apiServicesConfig.Get(ApiServicesConfig.SgfApi).Url);
            RestRequest request = new RestRequest("/Deuda/ObtenerFactura", Method.GET);
            request.AddHeader("Token", _apiServicesConfig.Get(ApiServicesConfig.SgfApi).Token);
            request.AddParameter("trxId", trxId);
            request.AddParameter("facElect", facElect);

            IRestResponse<dynamic> paymentsResponse = _restClient.Execute<dynamic>(request);
            if (!paymentsResponse.IsSuccessful)
            {
                Log.Error("No se pudo obtener Factura.\n trxId:{trxId} \n facElect:{facElect}\n Request: {@request} \n Response: {@response}", trxId, facElect, request, paymentsResponse);
            }

            var response = paymentsResponse.Data;

            return response;
        }

        public string UpdatePublishDebt(string cuit, string productCode, string publishDebt)
        {
            var requestModel = new UpdatePublishDebtRequest();
            if (!string.IsNullOrEmpty(cuit))
            {
                requestModel.Cuit = cuit;
                requestModel.ProductCode = productCode;
                requestModel.PublishDebt = publishDebt;
            }
            _restClient.BaseUrl = new Uri(_apiServicesConfig.Get(ApiServicesConfig.SgfApi).Url);
            RestRequest request = new RestRequest("/Deuda/ActualizarPublicaDeuda", Method.POST);
            request.AddHeader("Token", _apiServicesConfig.Get(ApiServicesConfig.SgfApi).Token);
            request.AddJsonBody(requestModel);
            try
            {
                IRestResponse<string> paymentsResponse = _restClient.Execute<string>(request);
                if (!paymentsResponse.IsSuccessful)
                {
                    Log.Error("No se pudo actualizar la publicación de deuda para Balance de cuenta.\n Cuit:{cuit} \n CodProducto: {productCode} \n PublicaDeuda: {publishDebt} \n Request: {@request} \n Response: {@response}", cuit, productCode, publishDebt, request, paymentsResponse);
                    throw new Exception($"Hubo un error al actualizar la publicacion de deuda. Cuit:{cuit} \n CodProducto: {productCode} \n PublicaDeuda: {publishDebt}");
                }

                var response = string.Empty;
                if (paymentsResponse.Data != null)
                {
                    response = paymentsResponse.Data;
                }
                return response;
            }
            catch (Exception ex)
            {
                Log.Error(ex, @"UpdatePublishDebt, 
                    Type:UpdatePublishDebtError,
                    Description: Error updating publishdebt:
                    request: {@request}", request);
                throw;
            }
        }

        public IEnumerable<string> GetBUListForCuits(List<string> cuits)
        {
            var result = new List<string>();
            var propertyCodes = _accountBalanceRepository.GetPropertyCodesByCuits(cuits).ToList();
            var buList = GetBusinessUnitsForPropertyCodes(propertyCodes).ToList();
            if (buList.Any(x => !string.IsNullOrEmpty(x)))
            {
                result.AddRange(buList.Distinct());
            }
            return result;
        }

        public List<DetalleDeuda> GetDebtsThatFitInMoneyAmount(IEnumerable<DetalleDeuda> debts, double moneyAmount)
        {
            var orderedDebts = debts.OrderBy(it => it.FechaPrimerVenc);
            var debtsForRet = new List<DetalleDeuda>();

            foreach (var detalleDeuda in orderedDebts)
            {
                var intPartEndIndex = detalleDeuda.ImportePrimerVenc.Length - 2;
                var strAmount = detalleDeuda.ImportePrimerVenc.Substring(0, intPartEndIndex) + "." +
                                detalleDeuda.ImportePrimerVenc.Substring(intPartEndIndex, 2);
                var aDebtAmount = strAmount.GetDouble();

                moneyAmount -= aDebtAmount;
                debtsForRet.Add(detalleDeuda);
                if (moneyAmount < 0)
                {
                    break;
                }
            }

            return debtsForRet;
        }

        public List<LogDto> GetLogFromMiddleware(string queryParams)
        {
            _restClient.BaseUrl = new Uri(_apiServicesConfig.Get(ApiServicesConfig.MiddlewareApi).Url);

            var request = new RestRequest($"/log{queryParams}", Method.GET)
            {
                RequestFormat = DataFormat.Json,
                Timeout = 60000,
            };

            try
            {
                IRestResponse<List<LogDto>> response = _restClient.Execute<List<LogDto>>(request);
                if (response.IsSuccessful)
                {
                    Log.Information("FinalizedPaymenInform to Middleware " +
                        "\n request: {@request} " +
                        "\n response:{@response}", request, response);
                    return response.Data;
                }
                else
                {
                    Log.Warning(@"FinalizedPaymenInform,
                        Type:FinalizedPaymenInformWarning,
                        Description: Response not successful:
                        request: {@request},
                        response: {@response}", request, response);
                    return new List<LogDto>();
                }
            }
            catch (Exception ex)
            {
                Log.Error(ex, @"FinalizedPaymenInform, 
                    Type:FinalizedPaymenInformError,
                    Description: Error when FinalizedPaymenInform to Middleware:
                    request: {@request}", request);
                throw;
            }
        }

        public List<DebtFreeNotificationDto> UpdateDebtFreeForNotifyList(DebtFreeNotificationDto debtFree)
        {
            var newDebtFreeList = new List<DebtFreeNotificationDto>();

            if (debtFree != null)
            {
                _restClient.BaseUrl = new Uri(_apiServicesConfig.Get(ApiServicesConfig.SgfApi).Url);

                var request = new RestRequest("/Cliente/ObtenerReporteLibreDeDeuda", Method.POST);
                request.AddHeader("Token", _apiServicesConfig.Get(ApiServicesConfig.SgfApi).Token);
                request.AddJsonBody(debtFree);

                IRestResponse<List<DebtFreeNotificationDto>> response = _restClient.Execute<List<DebtFreeNotificationDto>>(request);
                if (!response.IsSuccessful)
                {
                    Log.Error("No se pudo actualizar la lista de notificaciones de libre deuda pendientes. Error al informar la siguiente notificación como ya realizada: \n Cuit:{cuit} \n Razón Social:{razonSocial}\n CodProducto: {productCode} \n Email: {email}", debtFree.Cuit, debtFree.RazonSocial, debtFree.Producto, debtFree.Email);
                    throw new Exception($"Error al informar la siguiente notificación como ya realizada.\n Cuit:{debtFree.Cuit} \n Razón Social: {debtFree.RazonSocial} \n Producto: {debtFree.Producto} \n Email: {debtFree.Email}");
                }

                newDebtFreeList = response.Data;
            }

            return newDebtFreeList;
        }

        public async Task<List<DebtFreeNotificationDto>> GetDebtFreeForNotify()
        {
            var users = _userRepository.GetAllUsers();
            var usersDatacuit = users.Select(x => x.UserDataCuits).SelectMany(x => x).Select(x => x.Cuit)
                .Where(x => !string.IsNullOrEmpty(x)).Distinct().ToList();
             
            var rptDebtFree = await GetReporteLibreDeDeuda(usersDatacuit);
            var rptDebtFreeForNotify = rptDebtFree.Where(x => x.Ownership == "Si" && x.Notification == "N").ToList();

            var respose = new List<DebtFreeNotificationDto>();

            foreach (var debtFreeForNotify in rptDebtFreeForNotify)
            {
                var user = users.FirstOrDefault(x => x.Cuit == debtFreeForNotify.Cuit || x.UserDataCuits.Any(x => x.Cuit == debtFreeForNotify.Cuit));

                if (user is null)
                    continue;

                respose.Add(new DebtFreeNotificationDto
                {
                    RazonSocial = user.RazonSocial,
                    Cuit = debtFreeForNotify.Cuit,
                    Producto = debtFreeForNotify.Product,
                    Email = user.Email
                });
            }

            return respose;
        }

        private async Task<List<ReporteLibreDeDeudaResponse>> GetReporteLibreDeDeuda(List<string> cuits)
        {
            _restClient.BaseUrl = new Uri(_apiServicesConfig.Get(ApiServicesConfig.SgfApi).Url);
            var request = new RestRequest("/Cliente/ObtenerReporteLibreDeDeuda", Method.POST)
            {
                RequestFormat = DataFormat.Json
            };
            request.AddHeader("token", _apiServicesConfig.Get(ApiServicesConfig.SgfApi).Token);
            request.AddJsonBody(cuits);

            try
            {
                IRestResponse<List<ReporteLibreDeDeudaResponse>> response = await _restClient.ExecuteAsync<List<ReporteLibreDeDeudaResponse>>(request);
                if (response.IsSuccessful)
                {
                    Log.Information("GetReporteLibreDeDeuda to SGF ," +
                        "request: {@request} ," +
                        "response:{@response} ", request, response);
                    return response.Data;
                }
                else
                {
                    Log.Warning(@"GetReporteLibreDeDeuda,
                        Type:GetReporteLibreDeDeudaWarning,
                        Description: Response not successful:
                        request: {@request},
                        response: {@response}", request, response);
                    return new List<ReporteLibreDeDeudaResponse>();
                }
            }
            catch (Exception ex)
            {
                Log.Error(ex, @"GetReporteLibreDeDeuda, 
                    Type:GetReporteLibreDeDeudaError,
                    Description: Error when GetReporteLibreDeDeuda to SGF:
                    request: {@request}", request);
                throw;
            }
        }

        public async Task<string> UpdateNotificacionLibreDeuda(string cuit, string productCode)
        {
            var requestModel = new UpdateNotificacionLibreDeudaRequest()
            {
                Cuit = cuit,
                ProductCode = productCode
            };

            _restClient.BaseUrl = new Uri(_apiServicesConfig.Get(ApiServicesConfig.SgfApi).Url);
            var request = new RestRequest("/Deuda/ActualizarNotificacionLibreDeuda", Method.POST);
            request.AddHeader("Token", _apiServicesConfig.Get(ApiServicesConfig.SgfApi).Token);
            request.AddJsonBody(requestModel);

            try
            {
                IRestResponse<string> response = await _restClient.ExecuteAsync<string>(request);
                if (response.IsSuccessful)
                {
                    Log.Information("UpdateNotificacionLibreDeuda to SGF ," +
                        "request: {@request} ," +
                        "response:{@response} ", request, response);

                    return response.Data;
                }
                else
                {
                    Log.Warning(@"UpdateNotificacionLibreDeuda,
                        Type:UpdateNotificacionLibreDeudaWarning,
                        Description: Response not successful:
                        request: {@request},
                        response: {@response}", request, response);
                    return string.Empty;
                }
            }
            catch (Exception ex)
            {
                Log.Error(ex, @"UpdateNotificacionLibreDeuda, 
                    Type:UpdateNotificacionLibreDeudaError,
                    Description: Error when UpdateNotificacionLibreDeuda to SGF:
                    request: {@request}", request);
                throw;
            }
        }
    }
}
