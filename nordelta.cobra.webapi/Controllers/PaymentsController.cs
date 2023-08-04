using Microsoft.AspNetCore.Mvc;
using nordelta.cobra.webapi.Controllers.ActionFilters;
using nordelta.cobra.webapi.Models;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using nordelta.cobra.webapi.Services.Contracts;
using nordelta.cobra.webapi.Services.DTOs;
using Newtonsoft.Json;
using nordelta.cobra.webapi.Controllers.ViewModels;
using nordelta.cobra.webapi.Repositories.Contracts;
using Hangfire;
using nordelta.cobra.webapi.Models.ArchivoDeuda;
using Serilog;
using Microsoft.Extensions.Configuration;
using AutoMapper;
using static SQLite.SQLite3;

namespace nordelta.cobra.webapi.Controllers
{
    [ApiController]
    [ApiVersion("1.0")]
    [Route("api/v{version:apiVersion}/[controller]/[action]")]
    [Produces("application/json")]
    public class PaymentsController : ControllerBase
    {
        private readonly IDebinService _debinService;
        private readonly IPaymentService _paymentService;
        private readonly IArchivoDeudaRepository _archivoDeudaRepository;
        private readonly IExchangeRateFilesService _exchangeRateFilesService;
        private readonly IAnonymousPaymentsService _anonymousPaymentService;
        private readonly IPaymentsFilesService _paymentsFilesService;
        private readonly IConfiguration _configuration;
        private readonly IMapper _mapper;
        private readonly IPaymentReportsService _paymentReportsService;
        private readonly IAccountBalanceService _accountBalanceService;
        private readonly ICvuEntityService _cvuEntityService;
        private readonly IRestrictionsListService _restrictionsListService;
        private readonly IPaymentMethodService _paymentMethodService;
        private readonly IPaymentDetailService _paymentDetailService;
        private readonly INotificationService _notificationService;

        public PaymentsController(
            IDebinService debinService,
            IPaymentService paymentService,
            IArchivoDeudaRepository archivoDeudaRepository,
            IExchangeRateFilesService exchangeRateFilesService,
            IAnonymousPaymentsService anonymousPaymentService,
            IPaymentsFilesService paymentsFilesService,
            IConfiguration configuration,
            IMapper mapper,
            IPaymentReportsService paymentReportsService,
            IAccountBalanceService accountBalanceService,
            ICvuEntityService cvuEntityService,
            IRestrictionsListService restrictionsListService,
            IPaymentMethodService paymentMethodService,
            IPaymentDetailService paymentDetailService,
            INotificationService notificationService
        )
        {
            _debinService = debinService;
            _anonymousPaymentService = anonymousPaymentService;
            _paymentService = paymentService;
            _archivoDeudaRepository = archivoDeudaRepository;
            _exchangeRateFilesService = exchangeRateFilesService;
            _paymentsFilesService = paymentsFilesService;
            _configuration = configuration;
            _mapper = mapper;
            _paymentReportsService = paymentReportsService;
            _accountBalanceService = accountBalanceService;
            _cvuEntityService = cvuEntityService;
            _restrictionsListService = restrictionsListService;
            _paymentMethodService = paymentMethodService;
            _paymentDetailService = paymentDetailService;
            _notificationService = notificationService;
        }

        [AuthToken(new EPermission[] { EPermission.Access_Payments })]
        [HttpGet]
        public ActionResult<ExchangeRateFile> GetLastExchangeRate()
        {
            try
            {
                ExchangeRateFile result = _exchangeRateFilesService.GetLastExchangeRateFile();
                return new OkObjectResult(result);
            }
            catch (Exception ex)
            {
                Log.Error("Internal Error. GetLastExchangeRate, Exception detail: {@ex}", ex);
                return StatusCode(StatusCodes.Status500InternalServerError);
            }
        }

        [AuthToken(new EPermission[] { EPermission.Access_MyAccountBalance })]
        [HttpGet]
        public ActionResult<List<PaymentHistoryDto>> GetPaymentsHistory(string clientCuit, string productCode)
        {
            try
            {
                User user = ((User)JsonConvert.DeserializeObject(HttpContext.Request.Headers["user"], typeof(User)));
                string cuit = string.IsNullOrEmpty(clientCuit) ? user.Cuit.ToString() : clientCuit;
                var userCuits = user.AdditionalCuits.Append(user.Cuit.ToString()).ToList();

                if (!AuthTokenAttribute.HasPermissions(user, new EPermission[] { EPermission.Access_EverybodysPayments }) && !AuthTokenAttribute.HasPermissions(user, new EPermission[] { EPermission.Access_EverybodysPaymentsCriba }))
                    if (!userCuits.Contains(cuit))
                    {
                        Log.Debug(
                     @"Usuario no tiene permisos ejecutar el endpoint GetPaymentHistory con datos de otro CUIT. 
                        Usuario: {@user}, cuit:{cuit}, productCode:{productCode}", user, clientCuit, productCode);
                        return Unauthorized();
                    }

                List<PaymentHistoryDto> result = user.IsForeignCuit ? _paymentService.GetApplicationDetail(clientCuit, productCode, user.ClientReference)
                    : _paymentService.GetApplicationDetail(clientCuit, productCode);
                return new OkObjectResult(result);
            }
            catch (Exception ex)
            {
                Log.Error("Internal Error. GetPaymentsHistory, Exception detail: {@ex}", ex);
                return StatusCode(StatusCodes.Status500InternalServerError);
            }
        }

        [AuthToken(new EPermission[] { EPermission.Access_MyAccountBalance })]
        [HttpGet]
        public ActionResult<List<ResumenCuentaResponse>> GetPaymentsSummary(string clientCuit, string productCode)
        {
            try
            {
                User user = ((User)JsonConvert.DeserializeObject(HttpContext.Request.Headers["user"], typeof(User)));
                string cuit = string.IsNullOrEmpty(clientCuit) ? user.Cuit.ToString() : clientCuit;

                var userCuits = user.AdditionalCuits.Append(user.Cuit.ToString()).ToList();

                if (!AuthTokenAttribute.HasPermissions(user, new EPermission[] { EPermission.Access_EverybodysPayments }) && !AuthTokenAttribute.HasPermissions(user, new EPermission[] { EPermission.Access_EverybodysPaymentsCriba }))
                {
                    if (!userCuits.Contains(cuit))
                    {
                        Log.Debug(
                            @"Usuario no tiene permisos ejecutar el endpoint GetPaymentsSummary con datos de otro CUIT. 
                        Usuario: {@user}, cuit:{cuit}, productCode:{productCode}", user, clientCuit, productCode);
                        return Unauthorized();
                    }
                }

                List<ResumenCuentaResponse> result = user.IsForeignCuit ? _paymentService.GetBalanceDetail(cuit, productCode, user.ClientReference)
                    : _paymentService.GetBalanceDetail(cuit, productCode);

                return new OkObjectResult(result);
            }
            catch (Exception ex)
            {
                Log.Error("Internal Error. GetPaymentsSummary, Exception detail: {@ex}", ex);
                return StatusCode(StatusCodes.Status500InternalServerError);
            }
        }

        [AuthToken(new EPermission[] { EPermission.Access_AdvancePayments })]
        [HttpGet]
        public ActionResult<List<BalanceDetailForAdvanceResponse>> GetPaymentsForAdvance(string clientCuit, string productCode, string selectedCuit)
        {
            try
            {
                User user = ((User)JsonConvert.DeserializeObject(HttpContext.Request.Headers["user"], typeof(User)));
                string cuit = string.IsNullOrEmpty(clientCuit) ? user.Cuit.ToString() : clientCuit;

                var userCuits = user.AdditionalCuits.Append(user.Cuit.ToString()).ToList();

                if (!AuthTokenAttribute.HasPermissions(user, new EPermission[] { EPermission.Access_EverybodysPayments }) && !AuthTokenAttribute.HasPermissions(user, new EPermission[] { EPermission.Access_EverybodysPaymentsCriba }))
                {
                    if (!userCuits.Contains(cuit))
                    {
                        Log.Debug(
                            @"Usuario no tiene permisos ejecutar el endpoint GetPaymentsForAdvance con datos de otro CUIT. 
                        Usuario: {@user}, cuit:{cuit}, productCode:{productCode}", user, clientCuit, productCode);
                        return Unauthorized();
                    }
                }

                List<BalanceDetailForAdvanceResponse> result = _mapper.Map<List<BalanceDetailForAdvanceResponse>>(_paymentService.GetBalanceDetail(cuit, productCode));

                // CHEQUEAR CUOTAS QUE FIGURAN EN ARCHIVO DE DEUDA
                List<DetalleDeuda> deudas = _paymentService.GetAllPayments(cuit);

                result.ForEach(res =>
                {
                    res.OnDebtDetail = deudas.Any(x =>
                    {
                        DateTime fechaDeuda = DateTime.ParseExact(x.FechaPrimerVenc, "yyyyMMdd", null).Date;
                        DateTime fechaCuota = DateTime.ParseExact(res.Fecha, "dd/MM/yyyy", CultureInfo.InvariantCulture).Date;
                        return x.ObsLibreCuarta == res.Producto && fechaDeuda.Equals(fechaCuota);
                    });
                });

                // AGREGAR EL ESTADO DE LAS CUOTAS
                List<AdvanceFee> advanceFeeOrders = _archivoDeudaRepository.GetAdvancedFeesAsync().Result.Where(x => x.CodProducto == productCode && x.ClientCuit.ToString() == selectedCuit).ToList();

                result.ForEach(res =>
                {
                    var payment = advanceFeeOrders.FirstOrDefault(x =>
                        x.CodProducto == res.Producto && x.Vencimiento.Date.Equals(DateTime.ParseExact(res.Fecha, "dd/MM/yyyy", CultureInfo.InvariantCulture).Date));
                    if (payment == null) return;

                    res.Status = payment.Status;
                    res.CreatedOn = payment.CreatedOn;
                    res.RequestedByCuit = payment.ClientCuit.ToString();
                    res.AutoApproved = payment.AutoApproved.HasValue ? payment.AutoApproved.Value : false;
                });

                return new OkObjectResult(result);
            }
            catch (Exception ex)
            {
                Log.Error("Internal Error. GetPaymentsForAdvance, Exception detail: {@ex}", ex);
                return StatusCode(StatusCodes.Status500InternalServerError);
            }
        }

        [AuthToken(new EPermission[] { EPermission.Access_Payments })]
        [HttpGet]
        public ActionResult<IEnumerable<ArchivoDeuda>> GetPayments()
        {
            try
            {
                User user = ((User)JsonConvert.DeserializeObject(HttpContext.Request.Headers["user"], typeof(User)));
                string cuit = user.Cuit.ToString();

                if (!ModelState.IsValid)
                {
                    return BadRequest();
                }

                List<DetalleDeuda> result;

                if (AuthTokenAttribute.HasPermissions(user, new EPermission[] { EPermission.Access_EverybodysPayments }))
                {
                    result = _paymentService.GetAllPayments();
                }
                else if (user.AdditionalCuits?.Count >= 2) //Has multiple cuits
                {
                    user.AdditionalCuits.Add(cuit);
                    result = _paymentService.GetAllPayments(user.AdditionalCuits);
                }
                else
                {
                    result = user.IsForeignCuit ? _paymentService.GetAllPayments(cuit, user.AccountNumber) : _paymentService.GetAllPayments(cuit);
                }


                return new OkObjectResult(result);
            }
            catch (Exception ex)
            {
                Log.Error("Internal Error. GetPayments, Exception detail: {@ex}", ex);
                return StatusCode(StatusCodes.Status500InternalServerError);
            }
        }

        [AuthToken(new EPermission[] { EPermission.Access_Payments })]
        [HttpGet]
        public ActionResult<IEnumerable<PropertyCode>> GetPropertyCodes()
        {
            try
            {
                User user = GetUserFromHeader(HttpContext.Request.Headers["user"]);

                if (!ModelState.IsValid)
                {
                    return BadRequest();
                }

                List<string> cuitsToSearch = GetCuitsToSearch(user);

                var result = _paymentService.GetPropertyCodes(cuitsToSearch, user.IsForeignCuit ? user.AccountNumber : null);

                return new OkObjectResult(result);
            }
            catch (Exception ex)
            {
                Log.Error("Internal Error. GetPropertyCodes, Exception detail: {@ex}", ex);
                return StatusCode(StatusCodes.Status500InternalServerError);
            }

        }

        [AuthToken(new EPermission[] { EPermission.Access_MyAccountBalance })]
        [HttpGet]
        public ActionResult<IEnumerable<PropertyCode>> GetPropertyCodesForSummary()
        {
            try
            {
                User user = ((User)JsonConvert.DeserializeObject(HttpContext.Request.Headers["user"], typeof(User)));
                string cuit = user.Cuit.ToString();

                if (!ModelState.IsValid)
                {
                    return BadRequest();
                }

                List<PropertyCode> result;

                if (AuthTokenAttribute.HasPermissions(user, new EPermission[] { EPermission.Access_EverybodysPayments }))
                {
                    result = _paymentService.GetPropertyCodesForSummary(true, null, user);
                }
                else if (AuthTokenAttribute.HasPermissions(user,
                    new EPermission[] { EPermission.Access_EverybodysPaymentsCriba }))
                {
                    result = _paymentService.GetPropertyCodesForSummaryExternal(user);
                }
                else if (user.AdditionalCuits?.Count >= 2)
                {
                    result = _paymentService.GetPropertyCodesForSummary(false, user.AdditionalCuits, user);
                }
                else
                {
                    result = _paymentService.GetPropertyCodesForSummary(false, new List<string>() { cuit }, user);
                }

                return new OkObjectResult(result);
            }
            catch (Exception ex)
            {
                Log.Error("Internal Error. GetPropertyCodesForSummary, Exception detail: {@ex}", ex);
                return StatusCode(StatusCodes.Status500InternalServerError);
            }

        }
        [AuthToken(new EPermission[] { EPermission.Access_AdvancePayments })]
        [HttpGet]
        public ActionResult<IEnumerable<PropertyCode>> GetPropertyCodesForAdvance()
        {
            try
            {
                User user = ((User)JsonConvert.DeserializeObject(HttpContext.Request.Headers["user"], typeof(User)));
                string cuit = user.Cuit.ToString();

                if (!ModelState.IsValid)
                {
                    return BadRequest();
                }

                List<PropertyCode> result;

                if (AuthTokenAttribute.HasPermissions(user, new EPermission[] { EPermission.Access_EverybodysPayments }))
                {
                    result = _paymentService.GetPropertyCodesForAdvance(true, null);
                }
                else if (user.AdditionalCuits?.Count >= 2)
                {
                    result = _paymentService.GetPropertyCodesForAdvance(false, user.AdditionalCuits);
                }
                else
                {
                    result = _paymentService.GetPropertyCodesForAdvance(false, new List<string>() { cuit });
                }

                return new OkObjectResult(result);
            }
            catch (Exception ex)
            {
                Log.Error("Internal Error. GetPropertyCodesForAdvance, Exception detail: {@ex}", ex);
                return StatusCode(StatusCodes.Status500InternalServerError);
            }

        }

        [AuthToken(new EPermission[] { EPermission.Access_Payments })]
        [HttpGet]
        public ActionResult<IEnumerable<PropertyCodeFull>> GetPropertyCodesFull()
        {
            try
            {
                User user = ((User)JsonConvert.DeserializeObject(HttpContext.Request.Headers["user"], typeof(User)));

                List<PropertyCodeFull> results = new List<PropertyCodeFull>();

                if (AuthTokenAttribute.HasPermissions(user, new EPermission[] { EPermission.Access_EverybodysPayments }))
                {
                    results = _paymentService.GetPropertyCodesFull(); ;
                }

                return new OkObjectResult(results);
            }
            catch (Exception ex)
            {
                Log.Error("Internal Error. GetPropertyCodesFull, Exception detail: {@ex}", ex);
                return StatusCode(StatusCodes.Status500InternalServerError);
            }
        }

        [AuthToken(new EPermission[] { EPermission.Access_Payments })]
        [HttpGet]
        public ActionResult<IEnumerable<ArchivoDeuda>> GetPaymentsByFFileName(string Cuit, string FFileName)
        {
            try
            {
                User user = ((User)JsonConvert.DeserializeObject(HttpContext.Request.Headers["user"], typeof(User)));

                List<DetalleDeuda> results = new List<DetalleDeuda>();

                if (AuthTokenAttribute.HasPermissions(user, new EPermission[] { EPermission.Access_EverybodysPayments }))
                {
                    results = _paymentService.GetPaymentsByFFileName(Cuit, FFileName);
                }

                return new OkObjectResult(results);
            }
            catch (Exception ex)
            {
                Log.Error("Internal Error. GetPaymentsByFFileName, Exception detail: {@ex}", ex);
                return StatusCode(StatusCodes.Status500InternalServerError);
            }
        }

        [AuthToken(new EPermission[] { EPermission.Access_Payments })]
        [HttpPost]
        public ActionResult PublishDebin([FromBody] PublishDebinViewModel publishDebinViewModel)
        {
            try
            {
                if (!ModelState.IsValid || publishDebinViewModel.DebtAmounts.Count == 0)
                {
                    return BadRequest();
                }
                bool PublicacionDeDeudaIsRunning;
                using (var jobsConnection = JobStorage.Current.GetConnection())
                {
                    var runningJobs = JobStorage.Current.GetMonitoringApi().ProcessingJobs(0, int.MaxValue);
                    PublicacionDeDeudaIsRunning = runningJobs
                    .Any(x => x.Value.Job.Method.CustomAttributes
                    .Any(y => y.ConstructorArguments
                    .Any(z => z.Value?.ToString() == "PublicacionDeDeuda")));

                }
                User user = ((User)JsonConvert.DeserializeObject(HttpContext.Request.Headers["user"], typeof(User)));

                // If there is no PublicacionDeDeuda running, AND, all debts in request body is from last Publicacion, proceed
                if (!PublicacionDeDeudaIsRunning
                    && publishDebinViewModel.DebtAmounts.All(debt => _archivoDeudaRepository
                                                        .DetalleDeudaIsFromLastArchivoDeudaAvailable(debt.DebtId))
                    )
                    _debinService.PublishDebin(publishDebinViewModel, user).Wait();
                else
                    return new BadRequestObjectResult("Publicación de deuda desactualizada o en proceso");

                return new OkObjectResult("");
            }
            catch (Exception ex)
            {
                Log.Error("Internal Error. GetPaymentsByFFileName, Exception detail: {@ex}", ex);
                return new BadRequestObjectResult(ex.Message);
            }
        }

        [AuthToken(new EPermission[] { EPermission.Access_Extern_Payments })]
        [HttpPost]
        public async Task<ActionResult> PublishExternDebin([FromBody] ExternDebinViewModel externDebin)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    return BadRequest();
                }
                bool PublicacionDeDeudaIsRunning;
                using (var jobsConnection = JobStorage.Current.GetConnection())
                {
                    var runningJobs = JobStorage.Current.GetMonitoringApi().ProcessingJobs(0, int.MaxValue);
                    PublicacionDeDeudaIsRunning = runningJobs
                    .Any(x => x.Value.Job.Method.CustomAttributes
                    .Any(y => y.ConstructorArguments
                    .Any(z => (string)z.Value == "PublicacionDeDeuda")));
                }
                if (!PublicacionDeDeudaIsRunning)
                {
                    var result = await _anonymousPaymentService.PublishExternDebin(externDebin);
                    return new OkObjectResult(result);
                }
                else
                    return new BadRequestObjectResult("Publicación de deuda desactualizada o en proceso");
            }
            catch (Exception ex)
            {
                Log.Error("Internal Error. GetPaymentsByFFileName, Exception detail: {@ex}", ex);
                return new BadRequestObjectResult(ex.Message);
            }
        }

        [AuthToken(new EPermission[] { EPermission.Access_Extern_Payments })]
        [HttpGet]
        public async Task<ActionResult> GetDebinStatus(string debinCode)
        {
            try
            {
                var result = await _anonymousPaymentService.GetDebinStatus(debinCode);
                return new OkObjectResult(result);
            }
            catch (Exception e)
            {
                return new BadRequestObjectResult(e.Message);
            }
        }

        [AuthToken(new EPermission[] { EPermission.Access_MyAccountBalance })]
        [HttpGet]
        public ActionResult<string> GetReceipt(string clientCuit, string buId, string docId, string legalEntityId)
        {
            try
            {
                User user = ((User)JsonConvert.DeserializeObject(HttpContext.Request.Headers["user"], typeof(User)));
                string cuit = string.IsNullOrEmpty(clientCuit) ? user.Cuit.ToString() : clientCuit;
                var userCuits = user.AdditionalCuits.Append(user.Cuit.ToString()).ToList();

                if (!AuthTokenAttribute.HasPermissions(user, new EPermission[] { EPermission.Access_EverybodysPayments }) && !AuthTokenAttribute.HasPermissions(user, new EPermission[] { EPermission.Access_EverybodysPaymentsCriba }))
                    if (!userCuits.Contains(cuit))
                    {
                        Log.Debug(
                     @"Usuario no tiene permisos ejecutar el endpoint GetReceipt con datos de otro CUIT. 
                        Usuario: {@user}, cuit:{cuit}, buId:{buId}, docId:{docId}, legalEntityId:{legalEntityId}", user, clientCuit, buId, docId, legalEntityId);
                        return Unauthorized();
                    }

                var result = _paymentService.GetReceipt(buId, docId, legalEntityId);
                return new OkObjectResult(result);
            }
            catch (Exception ex)
            {
                Log.Error("Internal Error. GetReceipt, Exception detail: {@ex}", ex);
                return StatusCode(StatusCodes.Status500InternalServerError);
            }
        }

        [AuthToken(new EPermission[] { EPermission.Access_MyAccountBalance })]
        [HttpGet]
        public ActionResult<List<UnappliedPaymentDto>> GetUnappliedPayments(string clientCuit, string productCode)
        {
            try
            {
                User user = ((User)JsonConvert.DeserializeObject(HttpContext.Request.Headers["user"], typeof(User)));
                string cuit = string.IsNullOrEmpty(clientCuit) ? user.Cuit.ToString() : clientCuit;
                var userCuits = user.AdditionalCuits.Append(user.Cuit.ToString()).ToList();

                if (!AuthTokenAttribute.HasPermissions(user, new EPermission[] { EPermission.Access_EverybodysPayments }) && !AuthTokenAttribute.HasPermissions(user, new EPermission[] { EPermission.Access_EverybodysPaymentsCriba }))
                    if (!userCuits.Contains(cuit))
                    {
                        Log.Debug(
                     @"Usuario no tiene permisos ejecutar el endpoint GetUnappliedPayments con datos de otro CUIT. 
                        Usuario: {@user}, cuit:{cuit}, productCode:{productCode}", user, clientCuit, productCode);
                        return Unauthorized();
                    }

                List<UnappliedPaymentDto> result = user.IsForeignCuit ? _paymentService.GetUnappliedPayments(clientCuit, productCode, user.ClientReference)
                    : _paymentService.GetUnappliedPayments(clientCuit, productCode);
                return new OkObjectResult(result);
            }
            catch (Exception ex)
            {
                Log.Error("Internal Error. GetUnappliedPayments, Exception detail: {@ex}", ex);
                return StatusCode(StatusCodes.Status500InternalServerError);
            }
        }

        [AuthToken(new EPermission[] { EPermission.Access_MyAccountBalance })]
        [HttpGet]
        public ActionResult<string> GetInvoice(string clientCuit, string trxId, string facElect)
        {
            try
            {
                User user = ((User)JsonConvert.DeserializeObject(HttpContext.Request.Headers["user"], typeof(User)));

                string cuit = string.IsNullOrEmpty(clientCuit) ? user.Cuit.ToString() : clientCuit;
                var userCuits = user.AdditionalCuits.Append(user.Cuit.ToString()).ToList();

                if (!AuthTokenAttribute.HasPermissions(user, new EPermission[] { EPermission.Access_EverybodysPayments }))
                    if (!userCuits.Contains(cuit))
                    {
                        Log.Debug(
                     @"Usuario no tiene permisos ejecutar el endpoint GetInvoice con datos de otro CUIT. 
                        Usuario: {@user}, cuit:{cuit}, trxId:{trxId}, facElect:{facElect}", user, clientCuit, trxId, facElect);
                        return Unauthorized();
                    }

                var result = _paymentService.GetInvoice(trxId, facElect);
                return new OkObjectResult(result);
            }
            catch (Exception ex)
            {
                Log.Error("Internal Error. GetInvoice, Exception detail: {@ex}", ex);
                return StatusCode(StatusCodes.Status500InternalServerError);
            }
        }

        [AuthToken(new EPermission[] { EPermission.Access_Reports })]
        [HttpGet]
        public ActionResult<List<PublishDebtRejectionFile>> GetPublishDebtRejections([FromQuery] FilterReportByDatesViewModelRequest dates)
        {
            try
            {
                var result = _paymentsFilesService.GetPublishDebtRejections(dates);
                return new OkObjectResult(result);
            }
            catch (Exception ex)
            {
                Log.Error("Internal Error. GetPublishDebtRejections, Exception detail: {@ex}", ex);
                return StatusCode(StatusCodes.Status500InternalServerError);
            }
        }

        [AuthToken(new EPermission[] { EPermission.Access_Reports })]
        [HttpGet]
        public async Task<ActionResult<List<RepeatedDebsDetailsViewModel>>> GetAllRepeatedDebtDetails()
        {
            try
            {
                var result = await _paymentsFilesService.GetAllRepeatedDebtDetails();
                return new OkObjectResult(result);
            }
            catch (Exception ex)
            {
                Log.Error("Internal Error. GetAllRepeatedDebtDetails, Exception detail: {@ex}", ex);
                return StatusCode(StatusCodes.Status500InternalServerError);
            }
        }

        [AuthToken(new EPermission[] { EPermission.Access_AdvancePayments })]
        [HttpPost]
        public async Task<ActionResult> OrderAdvanceFee([FromBody] List<OrderAdvanceFeeViewModel> advanceFees)
        {
            try
            {
                if (_restrictionsListService.GetLockAdvancePayments().LockedByUser)
                    return StatusCode(StatusCodes.Status403Forbidden);

                User user = ((User)JsonConvert.DeserializeObject(HttpContext.Request.Headers["user"], typeof(User)));

                List<Restriction> userRestrictions = _restrictionsListService.GetRestrictionsListByUserId(user.Id);
                
                if (userRestrictions.Any(x => x.PermissionDeniedCode == EPermission.Access_AdvancePayments))
                    return StatusCode(StatusCodes.Status403Forbidden);


                var orderId = await _paymentsFilesService.PostOrderAdvanceFee(advanceFees, user);

                user.Id = string.IsNullOrEmpty(user.SupportUserId) ? user.Id : user.SupportUserId;
                user.Email = string.IsNullOrEmpty(user.SupportUserEmail) ? user.Email : user.SupportUserEmail;

                if (orderId != null && orderId > 0)
                {
                    await _paymentsFilesService.ChangeAdvanceFeeOrdersStatusAsync(new List<int> { orderId.Value }, EAdvanceFeeStatus.Aprobado);
                }

                return Ok();
            }
            catch (Exception ex)
            {
                Log.Error("Internal Error. OrderAdvanceFee, Exception detail: {@ex}", ex);
                return StatusCode(StatusCodes.Status500InternalServerError);
            }
        }

        [AuthToken(new EPermission[] { EPermission.Access_Admin_AdvancePayments })]
        [HttpGet]
        public async Task<ActionResult<IEnumerable<AdvanceFeeDto>>> GetAdvanceFeeOrders()
        {
            try
            {
                return Ok(await _paymentsFilesService.GetAdvanceFeeOrdersAsync());
            }
            catch (Exception ex)
            {
                Log.Error(ex, "Internal Error. GetAdvanceFeeOrders, Exception detail: {@ex}", ex);
                throw;
            }
        }

        [AuthToken(new EPermission[] { EPermission.Access_Admin_AdvancePayments })]
        [HttpPut]
        public async Task<ActionResult> ChangeAdvanceFeeOrdersStatus(List<dynamic> ids, EAdvanceFeeStatus status)
        {
            try
            {
                List<int> orderIds = new List<int>();

                foreach (var id in ids)
                {
                    orderIds.Add((int)id.orderId);
                }

                User user = ((User)JsonConvert.DeserializeObject(HttpContext.Request.Headers["user"], typeof(User)));

                user.Id = string.IsNullOrEmpty(user.SupportUserId) ? user.Id : user.SupportUserId;
                List<Restriction> userRestrictions = _restrictionsListService.GetRestrictionsListByUserId(user.Id);
                if (userRestrictions.Any(x => x.PermissionDeniedCode == EPermission.Access_AdvancePayments))
                    return StatusCode(StatusCodes.Status403Forbidden);

                await _paymentsFilesService.ChangeAdvanceFeeOrdersStatusAsync(orderIds, status);

                if (status == EAdvanceFeeStatus.Rechazado)
                {
                    _notificationService.NotifyRejectedAdvanceFeeOrders(ids);
                }
                return Ok();
            }
            catch (Exception ex)
            {
                Log.Error(ex, "Internal Error. ChangeAdvanceFeeOrdersStatus, Exception detail: {@ex}", ex);
                throw;
            }
        }


        [AuthToken(new EPermission[] { EPermission.Access_Reports })]
        [HttpGet]
        public ActionResult<List<PublishedDebtFile>> GetAllPublishedDebtFiles()
        {
            try
            {
                var result = _paymentsFilesService.GetAllPublishedDebtFiles();
                return new OkObjectResult(result);
            }
            catch (Exception ex)
            {
                Log.Error("Internal Error. GetAllPublishedDebtFiles, Exception detail: {@ex}", ex);
                return StatusCode(StatusCodes.Status500InternalServerError);
            }
        }

        [AuthToken(new EPermission[] { EPermission.Access_Payments })]
        [HttpGet]
        public ActionResult<IEnumerable<string>> GetBUListForClient()
        {
            try
            {
                User user = ((User)JsonConvert.DeserializeObject(HttpContext.Request.Headers["user"], typeof(User)));
                string cuit = user.Cuit.ToString();

                IEnumerable<string> result;

                if (user.AdditionalCuits?.Count > 1)
                {
                    result = _paymentService.GetBUListForCuits(user.AdditionalCuits);
                }
                else
                {
                    result = _paymentService.GetBUListForCuits(new List<string>() { cuit });
                }
                return new OkObjectResult(result);
            }
            catch (Exception ex)
            {
                Log.Error("Internal Error. GetBUListForClient, Exception detail: {@ex}", ex);
                return StatusCode(StatusCodes.Status500InternalServerError);
            }
        }

        [AuthToken(new EPermission[] { EPermission.Access_Payments })]
        [HttpPost]
        public ActionResult<PaymentReportCommandResponseDto> ReportPayments([FromBody] PaymentReportViewModel payment)
        {
            try
            {
                User user = ((User)JsonConvert.DeserializeObject(HttpContext.Request.Headers["user"], typeof(User)));
                if (user != null)
                {

                    var paymentReportDto = _mapper.Map<PaymentReportViewModel, PaymentReportDto>(payment);

                    return Ok(_paymentReportsService.CreatePaymentReport(paymentReportDto, user.Id));
                }
            }
            catch (Exception ex)
            {
                Log.Error("Internal Error. GetBUListForClient, Exception detail: {@ex}", ex);
            }
            return StatusCode(StatusCodes.Status500InternalServerError);
        }

        [AuthToken(new EPermission[] { EPermission.Access_Payments })]
        [HttpGet]
        public ActionResult<IEnumerable<CvuEntityViewModel>> GetCvuEntities(string clientCuit, string producto)
        {
            var cvuEntitiesViewModel = new List<CvuEntityViewModel>();
            try
            {
                var res = _accountBalanceService.GetAccountBalanceByCuitAndProduct(clientCuit, producto);

                if (res != null)
                {
                    var cvuEntities = _cvuEntityService.GetCvuEntitiesByIdAccounBalance(res.Id);
                    cvuEntitiesViewModel = _mapper.Map<IEnumerable<CvuEntityDto>, IEnumerable<CvuEntityViewModel>>(cvuEntities).ToList();
                }
            }
            catch (Exception ex)
            {
                Log.Error("Internal Error. GetCvuEntities, Exception detail: {@ex}", ex);
                return StatusCode(StatusCodes.Status500InternalServerError);
            }

            return cvuEntitiesViewModel;
        }

        [AuthToken(new EPermission[] { EPermission.Inform_Manually })]
        [HttpPost]
        public ActionResult InformPaymentMethodDoneManual([FromBody] List<ManuallyInformPaymentDto> manuallyInformPayments)
        {
            try
            {
                _debinService.InformPaymentDebinDoneManual(manuallyInformPayments);
                return Ok();
            }
            catch (Exception ex)
            {
                Log.Error("Internal Error. InformPaymentMethodDoneManual, Exception detail: {@ex}", ex);
                return StatusCode(StatusCodes.Status500InternalServerError);
            }
        }

        [AuthToken(new EPermission[] { EPermission.Access_Payments })]
        [HttpGet]
        public ActionResult<IEnumerable<PaymentMethodDto>> GetAllPaymentMethods()
        {
            try
            {
                return Ok(_paymentMethodService.GetPaymentMethods());
            }
            catch (Exception ex)
            {
                Log.Error("Internal Error. GetAllPaymentMethods, Exception detail: {@ex}", ex);
                return StatusCode(StatusCodes.Status500InternalServerError);
            }
        }

        [AuthToken(new EPermission[] { EPermission.Inform_Manually })]
        [HttpGet]
        public ActionResult GetAllDebines([FromQuery] int? limit, [FromQuery] int? page, [FromQuery] string payerId, [FromQuery] string fechaDesde, [FromQuery] string fechaHasta)
        {
            try
            {
                var pageSize = limit ?? 10;
                var pageNumber = page ?? 1;

                return Ok(_paymentMethodService.GetDebinesWithPagination(pageSize, pageNumber, payerId, fechaDesde, fechaHasta));
            }
            catch (Exception ex)
            {
                Log.Error("Internal Error. GetAllDebines, Exception detail: {@ex}", ex);
                return StatusCode(StatusCodes.Status500InternalServerError);
            }
        }

        [AuthToken(new EPermission[] { EPermission.Inform_Manually })]
        [HttpGet]
        public ActionResult<IEnumerable<User>> GetAllUsersFromDebin()
        {
            try
            {
                return Ok(_paymentMethodService.GetAllUsersFromDebin());
            }
            catch (Exception ex)
            {
                Log.Error("Internal Error. GetAllUsersFromDebin, Exception detail: {@ex}", ex);
                return StatusCode(StatusCodes.Status500InternalServerError);
            }
        }

        [AuthToken(new EPermission[] { EPermission.Access_Payments })]
        [HttpGet]
        public ActionResult<IEnumerable<PaymentDetailDto>> GetPaymentDetailsByPaymentMethodId(int paymentMethodId)
        {
            try
            {
                return Ok(_paymentDetailService.GetAllByPaymentMethodId(paymentMethodId));
            }
            catch (Exception ex)
            {
                Log.Error("Internal Error. GetPaymentDetailByPaymentMethodId, Exception detail: {@ex}", ex);
                return StatusCode(StatusCodes.Status500InternalServerError);
            }
        }

        [AuthToken(new EPermission[] { EPermission.Access_Admin_Payments })]
        [HttpGet]
        public ActionResult<IEnumerable<PaymentReportViewModel>> GetPaymentReportsByDate(DateTime fromDate, DateTime toDate)
        {
            try
            {
                User user = ((User)JsonConvert.DeserializeObject(HttpContext.Request.Headers["user"], typeof(User)));
                if (user != null)
                {
                    var paymentReports = _paymentReportsService.GetPaymentResportsByDate(fromDate, toDate);
                    return new OkObjectResult(paymentReports);
                }
            }
            catch (Exception ex)
            {
                Log.Error("Internal Error. GetPaymentReportsByDate, Exception detail: {@ex}", ex);
            }
            return StatusCode(StatusCodes.Status500InternalServerError);
        }

        [AuthToken(new EPermission[] { EPermission.Access_Debt_Free })]
        [HttpGet]
        public async Task<ActionResult<List<DebtFreeNotificationDto>>> GetDebtFreeForNotify()
        {
            try
            {
                return Ok(await _paymentService.GetDebtFreeForNotify());
            }
            catch (Exception ex)
            {
                Log.Error(ex, "Internal Error. GetDebtFreeForNotify, Exception detail: {msg}", ex.Message);
                return StatusCode(StatusCodes.Status500InternalServerError);
            }
        }

        [AuthToken(new EPermission[] { EPermission.Access_Debt_Free })]
        [HttpPost]
        public async Task<ActionResult> SendDebtFreeNotification(List<DebtFreeNotificationDto> debtFreeNotification)
        {
            try
            {
                var debtFreeList = new List<DebtFreeNotificationDto>();
                foreach (var debtFree in debtFreeNotification)
                {
                    var result = await _paymentService.UpdateNotificacionLibreDeuda(debtFree.Cuit, debtFree.Producto);
                    if(result.ToUpper().Trim() == "Y")
                    {
                        debtFreeList.Add(debtFree); 
                    }

                }

                if (debtFreeList.Any())
                {
                    _notificationService.NotifyDebtFreeUserReport(debtFreeList);
                    return Ok();
                }
                else
                {
                    return BadRequest("Could not update customer notificacionLibreDeuda attribute");
                }
            }
            catch (Exception ex)
            {
                Log.Error(ex, "Internal Error. SendDebtFreeNotification, Exception detail: {msg}", ex.Message);
                return StatusCode(StatusCodes.Status500InternalServerError);
            }
        }

        private User GetUserFromHeader(string headerValue)
        {
            return (User)JsonConvert.DeserializeObject(headerValue, typeof(User));
        }

        private List<string> GetCuitsToSearch(User user)
        {
            return user switch
            {
                _ when AuthTokenAttribute.HasPermissions(user, new[] { EPermission.Access_EverybodysPayments }) => default,
                _ when user.AdditionalCuits?.Count > 1 => user.AdditionalCuits,
                _ => new List<string> { user.Cuit.ToString() }
            };
        }
    }
}