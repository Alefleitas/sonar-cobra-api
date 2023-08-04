using nordelta.cobra.webapi.Services.Contracts;
using nordelta.cobra.webapi.Configuration;
using System.Text;
using Microsoft.Extensions.Options;
using nordelta.cobra.webapi.Repositories.Contracts;
using System.Collections.Generic;
using nordelta.cobra.webapi.Models.ArchivoDeuda;
using System.Linq;
using System;
using Microsoft.Extensions.Configuration;
using nordelta.cobra.webapi.Models;
using Serilog;
using System.Dynamic;
using System.Globalization;
using System.Net;
using RestSharp;
using nordelta.cobra.webapi.Controllers.Helpers;
using nordelta.cobra.webapi.Utils;
using Hangfire;
using nordelta.cobra.webapi.Services.DTOs;

namespace nordelta.cobra.webapi.Services
{
    public class NotificationService : INotificationService
    {
        private readonly IArchivoDeudaRepository _archivoDeudaRepository;
        private readonly IUserRepository _userRepository;
        private readonly INotificationRepository _notificationRepository;
        private readonly IDeliveryTypeInjector _deliveryTypeInjector;
        private readonly ICommunicationRepository _communicationRepository;
        private readonly IAccountBalanceRepository _accountBalanceRepository;
        private readonly IEmpresaRepository _empresaRepository;
        private readonly IExchangeRateFileRepository _exchangeRateFileRepository;
        private readonly IPaymentService _paymentService;//Need this service to get bu by product code
        private readonly IConfiguration _configuration;
        private readonly IRestClient _restClient;
        private readonly IOptionsMonitor<ApiServicesConfig> _apiServicesConfig;
        private readonly IMailService _mailService;
        private readonly IBackgroundJobClient _backgroundJobClient;

        public NotificationService(
            IArchivoDeudaRepository archivoDeudaRepository,
            IUserRepository userRepository,
            INotificationRepository notificationRepository,
            IDeliveryTypeInjector deliveryTypeInjector,
            ICommunicationRepository communicationRepository,
            IAccountBalanceRepository accountBalanceRepository,
            IEmpresaRepository empresaRepository,
            IExchangeRateFileRepository exchangeRateFileRepository,
            IPaymentService paymentService,
            IConfiguration configuration,
            IRestClient restClient,
            IOptionsMonitor<ApiServicesConfig> apiServicesConfig,
            IMailService mailService,
            IBackgroundJobClient backgroundJobClient
        )
        {
            _archivoDeudaRepository = archivoDeudaRepository;
            _userRepository = userRepository;
            _notificationRepository = notificationRepository;
            _deliveryTypeInjector = deliveryTypeInjector;
            _communicationRepository = communicationRepository;
            _accountBalanceRepository = accountBalanceRepository;
            _empresaRepository = empresaRepository;
            _exchangeRateFileRepository = exchangeRateFileRepository;
            _paymentService = paymentService;
            _configuration = configuration;
            _restClient = restClient;
            _apiServicesConfig = apiServicesConfig;
            _mailService = mailService;
            _backgroundJobClient = backgroundJobClient;
        }

        [DisableConcurrentExecution(timeoutInSeconds: 1800)]
        public void CheckForNotifications(Type type, DateTime date)
        {
            try
            {
                Log.Information("CheckForNotifications: running job...");

                var detallesDeuda = _archivoDeudaRepository.GetLastDetalleDeudasByCurrency("2").ToList(); //Solo se traen los DD con moneda USD
                var ssoUsers = _userRepository.GetAllUsers().ToList();
                var templateReferences = _notificationRepository.GetAllTemplateReferences();
                var nextCommunications = _communicationRepository.GetAll().ToList();

                var notificationTypes = _notificationRepository.GetNotificationTypes().Where(x => x.GetType() == type).ToList();

                var productCodes = detallesDeuda.Select(x => x.ObsLibreCuarta.Trim()).Distinct().ToList();
                var businessUnitByProductCode = _paymentService.GetBusinessUnitByProductCodeDictionary(productCodes);
                foreach (var notificationType in notificationTypes)
                {
                    if (notificationType.Template == null) continue;
                    var typeOfNotif = notificationType.GetType();
                    if (notificationType.Template.Disabled) continue;
                    notificationType.Delivery = this._deliveryTypeInjector.InjectService(notificationType.Delivery);
                    var notification = notificationType.Evaluate(detallesDeuda, null, nextCommunications, ssoUsers, date, out Dictionary<string, List<int>> dataMapper);
                    if (notification == null) continue;
                    foreach (var recipient in notification.Recipients)
                    {
                        var ssoUser = ssoUsers.Single(x => x.IdApplicationUser == recipient.Id);
                        var items = dataMapper[ssoUser.IdApplicationUser];
                        string subject;
                        string body;
                        if (typeOfNotif == typeof(PastDue) || typeOfNotif == typeof(FutureDue) || typeOfNotif == typeof(DayDue))
                        {
                            var detallesDeudaUser = detallesDeuda.Where(it => items.Contains(it.Id)).ToList();
                            var productCodesByUser = detallesDeudaUser.Select(it => it.ObsLibreCuarta.Trim()).Distinct().ToList();
                            //todo ver los que llegan en null or en blanco y mandar notificación a sistemas

                            NotifyNullOrEmptyBUs(productCodesByUser, businessUnitByProductCode);

                            var listBUs = productCodesByUser
                                .Select(productCode => businessUnitByProductCode[productCode])
                                .Where(emp => !string.IsNullOrEmpty(emp) && !string.IsNullOrWhiteSpace(emp)).Distinct()
                                .ToList();

                            foreach (var buName in listBUs)
                            {
                                var detallesDeudaEmpresa = detallesDeudaUser.Where(it =>
                                        businessUnitByProductCode[it.ObsLibreCuarta.Trim()] == buName)
                                    .OrderBy(it => it.ObsLibreCuarta.Trim())
                                    .ThenBy(it => DateTime.ParseExact(it.FechaPrimerVenc, "yyyyMMdd", null).Date)
                                    .ToList();

                                var detDeudaEmpresa = detallesDeudaEmpresa.FirstOrDefault();

                                subject = ReplaceTokens(notification.NotificationType.Template.Subject, ssoUser,
                                    detDeudaEmpresa, null, null, templateReferences,
                                    businessUnitByProductCode);

                                body = ReplaceTokens(notification.NotificationType.Template.HtmlBody, ssoUser,
                                    detDeudaEmpresa, null, null, templateReferences,
                                    businessUnitByProductCode, detallesDeudaEmpresa);
                                body += "<style>.ql-align-right{text-align: right;} .ql-align-center{text-align: center;} .ql-align-justify{text-align: justify;}</style>";
                                notification.NotificationType.Delivery.Send(ssoUser, subject, body);

                                PersistCommunications(ssoUser, notification.NotificationType.Description,
                                    detallesDeudaEmpresa);
                            }
                        }
                        else
                        {
                            foreach (var itemId in items)
                            {
                                var detalleDeuda = detallesDeuda.FirstOrDefault(x => x.Id == itemId);
                                var nextComm = nextCommunications.FirstOrDefault(x => x.Id == itemId);

                                if (typeOfNotif == typeof(NextCommunication))
                                {
                                    detalleDeuda = detallesDeuda.FirstOrDefault(x =>
                                        nextComm != null && x.ObsLibreCuarta.Trim() == nextComm.AccountBalance.Product);
                                    subject = ReplaceTokens(notification.NotificationType.Template.Subject, ssoUser, detalleDeuda, null, nextComm, templateReferences, businessUnitByProductCode);
                                    body = ReplaceTokens(notification.NotificationType.Template.HtmlBody, ssoUser, detalleDeuda, null, nextComm, templateReferences, businessUnitByProductCode);
                                }
                                else
                                {
                                    subject = ReplaceTokens(notification.NotificationType.Template.Subject, ssoUser, detalleDeuda, null, null, templateReferences, businessUnitByProductCode);
                                    body = ReplaceTokens(notification.NotificationType.Template.HtmlBody, ssoUser, detalleDeuda, null, null, templateReferences, businessUnitByProductCode);
                                }
                                //Add styles for alignText
                                body += "<style>.ql-align-right{text-align: right;} .ql-align-center{text-align: center;} .ql-align-justify{text-align: justify;}</style>";
                                notification.NotificationType.Delivery.Send(ssoUser, subject, body);
                                PersistCommunications(ssoUser, notification.NotificationType.Description, detalleDeuda);
                            }
                        }

                    }
                    _notificationRepository.Save(notification);
                }
            }
            catch (Exception e)
            {
                Log.Error("CheckForPaymentsNotificationsError: Error searching for notifications, ERROR: {error}", e.Message);
            }
        }
        public bool NotifyNullOrEmptyBUs(List<string> productCodesByUser, Dictionary<string, string> businessUnitByProductCode)
        {
            List<string> voidIndex = new List<string>();
            foreach (string productCode in productCodesByUser)
            {
                string value = businessUnitByProductCode.FirstOrDefault((t => t.Key == productCode)).Value;
                if (string.IsNullOrEmpty(value) || string.IsNullOrWhiteSpace(value))
                    voidIndex.Add(productCode);
            }

            if (voidIndex.Count == 0)
                return false;

            _backgroundJobClient.Enqueue(() => SendMailBusNulls(voidIndex));
            return true;
        }

        public void SendMailBusNulls(List<string> listBUsNullOrEmpty)
        {
            var body = "";
            foreach (string bu in listBUsNullOrEmpty)
                body += $"<h5>-{bu}</h5>";
            var missingBUsEmails = _configuration.GetSection("ServiceConfiguration:RecipientsMissingBUs").Get<List<string>>();
            _mailService.SendNotificationEmail(missingBUsEmails, $"Productos sin BU ({listBUsNullOrEmpty.Count})", $"Este es un email automático de Cobra. Se detectaron {listBUsNullOrEmpty.Count} productos sin BU: {body}");
        }

        public void CheckForDebinNotifications(List<Debin> debins)
        {
            try
            {
                Log.Information("CheckForDebinNotifications: running job with debins {@debins}", debins);

                var ssoUsers = _userRepository.GetUsersByIds(debins.GroupBy(x => x.Payer.Id).Select(grp => grp.Key).ToList());
                var templateReferences = _notificationRepository.GetAllTemplateReferences();
                //I get all detalleDeudas associated to debins
                var detalleDeudas = _archivoDeudaRepository.GetDetalleDeudasByDebinIds(debins.Select(x => x.Id).ToList());

                List<NotificationType> notificationTypes = _notificationRepository.GetNotificationTypes();
                //Get all productCodes associated to detalleDeudas and then get all businessUnits associated with those productCodes
                List<string> productCodes = detalleDeudas.Select(x => x.ObsLibreCuarta.Trim()).Distinct().ToList();
                Dictionary<string, string> businessUnitByProductCode = _paymentService.GetBusinessUnitByProductCodeDictionary(productCodes);

                foreach (NotificationType notificationType in notificationTypes)
                {
                    if (notificationType.Template != null)
                    {
                        if (!notificationType.Template.Disabled)
                        {
                            notificationType.Delivery = this._deliveryTypeInjector.InjectService(notificationType.Delivery);
                            Notification notification = notificationType.Evaluate(null, debins, null, ssoUsers, LocalDateTime.GetDateTimeNow(), out Dictionary<string, List<int>> dataMapper);
                            if (notification != null)
                            {
                                foreach (var recipient in notification.Recipients)
                                {
                                    var ssoUser = ssoUsers.Where(x => x.IdApplicationUser == recipient.Id).Single();
                                    var items = dataMapper[ssoUser.IdApplicationUser];
                                    foreach (var itemId in items)
                                    {
                                        var debin = debins.Where(x => x.Id == itemId).FirstOrDefault();
                                        var detalleDeuda = detalleDeudas.Where(x => x.PaymentMethod.Id == debin.Id).FirstOrDefault();//Added so I can pass productCode to GetEmailBody
                                        var subject = ReplaceTokens(notification.NotificationType.Template.Subject, ssoUser, detalleDeuda, debin, null, templateReferences, businessUnitByProductCode);
                                        var body = ReplaceTokens(notification.NotificationType.Template.HtmlBody, ssoUser, detalleDeuda, debin, null, templateReferences, businessUnitByProductCode);
                                        //Add styles for alignText
                                        body += "<style>.ql-align-right{text-align: right;} .ql-align-center{text-align: center;} .ql-align-justify{text-align: justify;}</style>";
                                        notification.NotificationType.Delivery.Send(ssoUser, subject, body);
                                        PersistCommunications(ssoUser, notification.NotificationType.Description, debin.Debts.FirstOrDefault());
                                    }
                                }
                                _notificationRepository.Save(notification);
                            }
                            dataMapper = new Dictionary<string, List<int>>();
                        }
                    }
                }
            }
            catch (Exception e)
            {
                Log.Error("CheckForDebinNotificationsError: Error searching for debin notifications, ERROR: {error}", e.Message);
            }
        }

        public void NotifyQuotationCancellation(int quotationId)
        {
            var quotation = _exchangeRateFileRepository.GetQuotationById(quotationId);
            Log.Information("NotifyQuotation: new quotation for cancellation: {@q}", quotation);
            var roles = _configuration.GetSection("RolesForQuotationCancellation").Get<List<string>>();
            var ssoUsers = _userRepository.GetUsersByRoles(roles);
            var cobraUrl = _apiServicesConfig.Get(ApiServicesConfig.CobraApi).Url;

            var encryptedData = AesManager.EncryptForUrl(quotationId.ToString(), _configuration.GetSection("JwtKey").Value);
            var hash = $"{encryptedData.encrypted}.{encryptedData.iv}";
            var encodedHash = WebUtility.UrlEncode(hash);
            var message = $"<p>Se cargó la cotización <strong>{quotation.GetType().Name}</strong> de tipo <strong>{quotation.RateType}</strong> con el valor: <strong>${quotation.Valor}</strong>." +
                        $"Para cancelar la carga presione el botón.<br /><br />" +
                        $"<form target='_blank' action = '{cobraUrl}/Quotation/CancellQuotation/{encodedHash}'><input type='submit' value='Cancelar Cotización' /></form>";
            var recipients = ssoUsers.Select(x => x.Email).ToList();
            _mailService.SendNotificationEmail(recipients, "Cancelacion de Cotizacion", message);
        }

        public bool CreateTemplate(Template template, int notificationTypeId)
        {
            try
            {
                var notificationType = _notificationRepository.GetNotificationTypeById(notificationTypeId);
                notificationType.Template = template;
                _notificationRepository.UpdateNotificationType(notificationType);
                return true;
            }
            catch (Exception ex)
            {
                Log.Error(ex, "Error in CreateTemplate");
                throw;
            }
        }

        public bool UpdateTemplate(Template template)
        {
            try
            {
                var dbTemplate = _notificationRepository.GetTemplateById(template.Id);
                dbTemplate.Description = template.Description;
                dbTemplate.HtmlBody = template.HtmlBody;
                dbTemplate.Subject = template.Subject;
                _notificationRepository.UpdateTemplate(dbTemplate);
                return true;
            }
            catch (Exception ex)
            {
                Log.Error(ex, "Error in UpdateTemplate");
                throw;
            }
        }

        private void PersistCommunications(SsoUser client, string subject, DetalleDeuda detalleDeuda)
        {
            if (detalleDeuda != null)
            {
                AddCommunication(client, subject, detalleDeuda);
            }
        }
        private void PersistCommunications(SsoUser client, string subject, List<DetalleDeuda> detallesDeuda)
        {
            foreach (var detalleDeuda in detallesDeuda)
            {
                AddCommunication(client, subject, detalleDeuda);
            }
        }

        private void AddCommunication(SsoUser client, string subject, DetalleDeuda detalleDeuda)
        {
            var accountBalance =
                _accountBalanceRepository.GetAccountBalance(detalleDeuda.NroCuitCliente, detalleDeuda.ObsLibreCuarta);
            if (accountBalance != null)
            {
                var accountBalanceId = accountBalance.Id;

                var communications = new List<Communication>();
                if (client.Roles.All(x => x.Role != "Cliente")) return;
                var comm = new Communication
                {
                    Id = 0,
                    Client = new User { Id = client.IdApplicationUser, Email = client.Email },
                    SsoUser = new SsoUser { IdApplicationUser = client.IdApplicationUser, Email = client.Email },
                    CommunicationChannel = EComChannelType.CorreoElectronico,
                    Date = LocalDateTime.GetDateTimeNow(),
                    Incoming = false,
                    Description = subject,
                    AccountBalanceId = accountBalanceId,
                    CommunicationCreatorUserId = "System"
                };
                _communicationRepository.InsertOrUpdate(comm);
            }
            else
            {
                Log.Warning(
                    "Notifications WARNING: No se encontro AccountBalance para el detalle deuda, por ende no se persiste Communication. DetalleDeuda: {@detalledeuda}",
                    detalleDeuda);
            }
        }

        public string ReplaceTokens(string stringWithTokens, SsoUser ssoUser, DetalleDeuda detalleDeuda, Debin debin, Communication comm, List<TemplateTokenReference> templateReferences, Dictionary<string, string> businessUnitByProductCode, List<DetalleDeuda> detallesDeuda = null)
        {
            var finalString = stringWithTokens;
            var objectProperties = templateReferences
                .Where(templateReference => stringWithTokens.Contains(templateReference.Token)).ToDictionary(
                    templateReference => templateReference.Token,
                    templateReference => templateReference.ObjectProperty);

            if (objectProperties.Count <= 0) return finalString;
            var mapObject = new ExpandoObject() as IDictionary<string, object>;
            objectProperties.Select(x => x.Value).ToList().ForEach(x => mapObject.Add(x, null));
            var objectPropertiesValues = objectProperties.Select(x => x.Value).ToList();

            //Gets all possible info to replace in the template
            foreach (var objectProperty in objectPropertiesValues)
            {
                var businessUnit = detalleDeuda != null ? businessUnitByProductCode[detalleDeuda?.ObsLibreCuarta.Trim()] : null;
                var empresa = businessUnit != null ? _empresaRepository.GetByName(businessUnit) : null;
                var nfi = (NumberFormatInfo)CultureInfo.InvariantCulture.NumberFormat.Clone();
                nfi.NumberGroupSeparator = ".";
                nfi.NumberDecimalSeparator = ",";
                switch (objectProperty)
                {
                    case "NroComprobante":
                        mapObject[objectProperty] = detalleDeuda?.NroComprobante;
                        break;
                    case "ImportePrimerVenc":

                        nfi.NumberGroupSeparator = ".";
                        nfi.NumberDecimalSeparator = ",";
                        mapObject[objectProperty] = detalleDeuda != null ? (Convert.ToDecimal(detalleDeuda.ImportePrimerVenc) / 100).ToString("n", nfi) : null;
                        break;
                    case "CodigoMoneda":
                        mapObject[objectProperty] = detalleDeuda?.CodigoMoneda == "0" ? "ARS" : "USD";
                        break;
                    case "FechaPrimerVenc":
                        mapObject[objectProperty] = detalleDeuda != null ? DateTime.ParseExact(detalleDeuda.FechaPrimerVenc, "yyyyMMdd", new CultureInfo("es-ES")).ToString("dd MMMM", new CultureInfo("es-ES")) : null;
                        break;
                    case "NroCuota":
                        mapObject[objectProperty] = detalleDeuda?.NroCuota;
                        break;
                    case "DebinCode":
                        mapObject[objectProperty] = debin?.DebinCode;
                        break;
                    case "RazonSocial":
                        mapObject[objectProperty] = comm != null ? _userRepository.GetSsoUserById(comm.Client.Id).RazonSocial : ssoUser.RazonSocial;
                        break;
                    case "Cuit":
                        mapObject[objectProperty] = ssoUser.Cuit;
                        break;
                    case "CodProducto":
                        mapObject[objectProperty] = detalleDeuda?.ObsLibreCuarta.Trim();
                        break;
                    case "FirmaBU":
                        mapObject[objectProperty] = $"<img src=\"{empresa?.Firma}\">";
                        break;
                    case "CorreoBU":
                        mapObject[objectProperty] = empresa?.Correo;
                        break;
                    case "TablaDetalleDeuda":
                        if (detallesDeuda != null)
                        {

                            var body = @"
                                    <table cellpadding='0' cellspacing='0' style='min-width: 600px;'>
                                         <tr>
                                            <th style='border-top-left-radius: 5px !important;'>Producto</th>
                                            <th>Moneda</th>
                                            <th>Importe</th>
                                            <th style='border-top-right-radius: 5px !important;'>Vencimiento</th>
                                        </tr>
                                        {{tbody}}
                                    </table>                                    
                                  ";
                            var lineas = "";
                            foreach (var item in detallesDeuda)
                            {
                                lineas += $"<tr>" +
                                          $"<td>{item.ObsLibreCuarta}</td>" +
                                          $"<td>{(item.CodigoMoneda == "0" ? "ARS" : "USD")}</td>" +
                                          $"<td>{(Convert.ToDecimal(item.ImportePrimerVenc) / 100).ToString("n", nfi)}</td>" +
                                          $"<td>{DateTime.ParseExact(item.FechaPrimerVenc, "yyyyMMdd", new CultureInfo("es-ES")).ToString("dd MMMM yyyy", new CultureInfo("es-ES"))}" +
                                          $"</tr>";
                            }

                            body = body.Replace("{{tbody}}", lineas);
                            mapObject[objectProperty] = body;

                        }
                        break;
                    case "NombreEmpresa":
                        mapObject[objectProperty] = empresa != null ? empresa.Nombre : string.Empty;
                        break;


                }
            }

            //tokens replacement
            foreach (var objectProperty in objectProperties)
            {
                object obj = new { };
                if (mapObject[objectProperty.Value] != null)
                {
                    object value = mapObject.TryGetValue(objectProperty.Value, out value);
                    finalString = (bool)value ? finalString.Replace(objectProperty.Key, mapObject[objectProperty.Value].ToString()) :
                        finalString.Replace(objectProperty.Key, "[NO_INFO_FOR_" + objectProperty.Value + "]");
                }
            }
            return finalString;
        }

        public void NotifyRejectedAdvanceFeeOrders(List<dynamic> orders)
        {
            int orderId = 0;
            string motivoRechazo = string.Empty;
            int result = 0;
            var recipients = new List<string>();
            var advanceFeeList = new List<AdvanceFee>();
            var updateInformedStatus = new List<int>();

            var htmlTemplate = _notificationRepository.GetTemplateByDescInternal(TemplateDescription.RejectedAdvanceeFee).HtmlBody;


            try
            {
                foreach (var item in orders)
                {

                    if (item.orderId != null && int.TryParse(item.orderId.ToString(), out result))
                    {
                        orderId = result;
                        updateInformedStatus.Add(orderId);
                    }

                    if (item.motivoRechazo != null)
                    {
                        motivoRechazo = item.motivoRechazo.ToString();
                    }

                    var advanceFee = _archivoDeudaRepository.GetAdvancedFeesByOrderId(orderId, x => x.Informed.Value && x.Status == EAdvanceFeeStatus.Aprobado || !x.Informed.Value);

                    if (advanceFee != null)
                    {
                        advanceFee.MotivoRechazo = motivoRechazo;
                        advanceFeeList.Add(advanceFee);
                    }
                }

                var groupedByUserAndProduct = advanceFeeList.GroupBy(x => x.UserId).Select(x => new { UserId = x.Key, Products = x.GroupBy(y => y.CodProducto) });
                var buList = new List<ProductCodeBusinessUnitDTO>();
                foreach (var group in groupedByUserAndProduct)
                {
                    var user = _userRepository.GetUserById(group.UserId);
                    StringBuilder fechaVencimientoStr = new StringBuilder();

                    recipients.Add(user.Email);

                    var products = group.Products.ToList();

                    foreach (var prod in products)
                    {
                        StringBuilder sb = new StringBuilder(htmlTemplate);
                        if (user != null)
                        {
                            sb.Replace("{CLIENTE_NOMBRE}", $"{user.FirstName}{user.LastName}");
                            sb.Replace("{{CODIGO_PRODUCTO}}", prod.Key);
                            foreach (var advanceFee in prod)
                            {
                                fechaVencimientoStr.Append($"\"Cuota con fecha de vencimiento {advanceFee.Vencimiento.ToString("d")}\"");
                                fechaVencimientoStr.AppendLine();

                                buList.AddRange(_paymentService.GetBusinessUnitByProductCodes(new List<string> { advanceFee.CodProducto }));
                            }

                            sb.Replace("{{CUOTA_VENCIMIENTO}}", fechaVencimientoStr.ToString() + "<br/>");
                            var empresa = _empresaRepository.GetByName(buList?.FirstOrDefault().BusinessUnit);
                            buList = new List<ProductCodeBusinessUnitDTO>();
                            string firma = $"<img src=\"{empresa?.Firma}\">";
                            sb.Replace("{motivo_Rechazo}", prod.First().MotivoRechazo)
                                .Replace("{{FIRMA_BU}}", firma);
                            string finalHtml = sb.ToString();
                            _mailService.SendNotificationEmail(recipients, $"Adelantos de Cuotas Rechazadas", finalHtml);
                            fechaVencimientoStr.Clear();
                        }
                        else
                        {
                            Serilog.Log.Information("No AdvanceFeeOrders where found for notification");
                        }
                    }
                    recipients = new List<string>();
                }
            }
            catch (Exception ex)
            {
                Serilog.Log.Error(@"Error Running method NotifyRejectedAdvanceFeeOrders: {@err}", ex);

            }
            finally
            {

                _archivoDeudaRepository.UpdateInformedAdvanceFeeByIds(updateInformedStatus);
            }

        }

        public void NotifyAdvanceFeeOrders(EAdvanceFeeStatus status)
        {
            Log.Information(@"Running Job NotifyAdvanceFeeOrders for status {status}");
            var sent = new List<int>();
            try
            {
                var advanceFees = _archivoDeudaRepository.GetAdvancedFeesAsync(status).Result.ToList();

                if (advanceFees.Count > 0)
                {
                    //Get BUs by productCodes
                    var businessUnitByProductCode =
                        _paymentService.GetBusinessUnitByProductCodes(advanceFees.Select(x => x.CodProducto.Trim())
                            .Distinct().ToList());
                    //Get users with the specified roles to notify
                    var result = (from af in advanceFees
                                  join bu in businessUnitByProductCode on af.CodProducto.Trim() equals bu.Codigo.Trim()
                                  select new
                                  {
                                      AdvanceFeeId = af.Id,
                                      CuitCliente = af.ClientCuit,
                                      Importe = af.Importe,
                                      Moneda = af.Moneda,
                                      Vencimiento = af.Vencimiento,
                                      Producto = af.CodProducto.Trim(),
                                      BU = bu.BusinessUnit
                                  }).ToList();

                    var usersToNotify = new List<SsoUser>();
                    var recipients = new List<string>();
                    var roles = new List<string>();

                    if (status == EAdvanceFeeStatus.Pendiente)
                    {
                        //Get cuentas a cobrar users by role
                        roles = _configuration.GetSection("ServiceConfiguration:RolesForAdvanceFeeOrders")
                            .Get<List<string>>();
                        usersToNotify = _userRepository.GetUsersByRoles(roles);
                    }
                    else if (status == EAdvanceFeeStatus.Aprobado)
                    {
                        //Get additional billing department email addresses to notify
                        recipients = _configuration.GetSection("ServiceConfiguration:BillingEmailAddresses")
                            .Get<List<string>>();
                    }

                    var groupedResult = result.GroupBy(x => x.BU).ToList();

                    //var recipients = new List<string>();

                    var nfi = (NumberFormatInfo)CultureInfo.InvariantCulture.NumberFormat.Clone();
                    nfi.NumberGroupSeparator = ".";
                    nfi.NumberDecimalSeparator = ",";

                    foreach (var group in groupedResult)
                    {
                        var sb = new StringBuilder();
                        var usersWithTheProperBu = usersToNotify
                            .Where(u => u.Empresas.Select(e => e.Empresa).ToList().Contains(group.Key))
                            .Select(x => x.Email).ToList();
                        recipients.AddRange(usersWithTheProperBu);
                        foreach (var item in group)
                        {
                            sb.Append(
                                $"<p>El cliente con cuit {item.CuitCliente} solicita adelanto de cuota del producto {item.Producto} " +
                                $"con vencimiento {item.Vencimiento:dd/MM/yyyy} y un saldo de {item.Moneda} {(Convert.ToDecimal(item.Importe)).ToString("n", nfi)}</p>");
                            sent.Add(item.AdvanceFeeId);
                        }

                        if (!string.IsNullOrEmpty(group.Key))
                        {
                            sb.Append($"<img src=\"{_empresaRepository.GetByName(group.Key)?.Firma}\">");
                        }

                        _mailService.SendNotificationEmail(recipients, $"Adelantos de Cuotas {status.ToString()}s", sb.ToString());
                    }
                }
                else
                {
                    Serilog.Log.Information("No AdvanceFeeOrders where found for notification");
                }
            }
            catch (Exception ex)
            {
                Serilog.Log.Error(@"Error Running Job NotifyAdvanceFeeOrders: {@err}", ex);
            }
            finally
            {
                if (status == EAdvanceFeeStatus.Aprobado || status == EAdvanceFeeStatus.Rechazado)
                    _archivoDeudaRepository.UpdateInformedAdvanceFeeByIds(sent);
            }
        }

        public void NotifyTodayQuotations()
        {
            List<Quotation> quotations = _exchangeRateFileRepository.GetAllQuotationsToday();

            try
            {
                var cobraRoles = _configuration.GetSection("CobraRolesForQuotationNotification").Get<List<string>>();
                var sgcRoles = _configuration.GetSection("SgcRolesForQuotationNotification").Get<List<string>>();
                string body = "Cotizaciones cargadas:<br><br>";

                foreach (Quotation quotation in quotations)
                    body += $"{quotation.GetType().Name} {quotation.RateType} {Convert.ToString(quotation.EffectiveDateFrom.Date.ToShortDateString())}: <b>{quotation.Valor}</b><br>";

                // Cobra
                var userMailsCobra = _userRepository.GetUsersByRoles(cobraRoles).Select(x => new QuotationNotificationUser()
                {
                    Id = x.Id.ToString(),
                    Email = x.Email,
                });

                //Inform only if there are mails and quotations
                if (quotations.Count != 0 && userMailsCobra.Count() != 0)
                    _backgroundJobClient.Enqueue(() =>
                    _mailService.SendNotificationEmail(userMailsCobra.Select(x => x.Email).ToList(), $"Se han cargado nuevas cotizaciones", body));

                // SGC
                _restClient.BaseUrl = new Uri(_apiServicesConfig.Get(ApiServicesConfig.SgcApi).Url);
                RestRequest request = new RestRequest("/Usuario/Roles", Method.POST)
                {
                    RequestFormat = DataFormat.Json
                };
                request.AddHeader("token", _apiServicesConfig.Get(ApiServicesConfig.SgcApi).Token);
                request.AddJsonBody(sgcRoles);

                IRestResponse<List<QuotationNotificationUser>> response = AsyncHelper.RunSync(
                    async () => await _restClient.ExecuteAsync<List<QuotationNotificationUser>>(request));

                if (response.IsSuccessful)
                {
                    var sgcUsers = response.Data;

                    var userMailsSgc = sgcUsers.Select(x => new QuotationNotificationUser()
                    {
                        Id = x.Id.ToString(),
                        Email = x.Email,
                    });

                    //Inform only if there are mails and quotations
                    if (quotations.Count != 0 && userMailsSgc.Count() != 0)
                    {
                        _backgroundJobClient.Enqueue(() =>
                        _mailService.SendNotificationEmail(userMailsSgc.Select(x => x.Email).ToList(), $"Se han cargado nuevas cotizaciones", body));
                    }
                }
                else
                    Log.Error("NotifyTodayQuotationsERROR: Error on request to SGC. request: {@request}, @response: {}", request, response);

            }
            catch (Exception ex)
            {
                Log.Error(@"Error in getting today's quotations: {@err}", ex);
            }
        }

        public void NotifyDebtFreeUserReport(List<DebtFreeNotificationDto> debtsFree)
        {
            try
            {
                var recipients = _configuration.GetSection("ServiceConfiguration:FreeDebtUserReportRecipient")
                                               .Get<List<FreeDebtUserRecipient>>();

                var htmlTemplate = _notificationRepository.GetTemplateByDescInternal(TemplateDescription.FreeDebtUserReportTemplate).HtmlBody;

                var usuariosPorBusinessUnit = new Dictionary<string, List<User>>();

                if (debtsFree == null || debtsFree.Count == 0)
                    return;

              

                foreach (var debts in debtsFree)
                {
                    var userBU = _paymentService.GetBusinessUnitByProductCodes(new List<string> { debts.Producto });
                    var user = _userRepository.GetUserByCuit(debts.Cuit) ??
                               throw new Exception($"NotifyLibreDeuda Error: No client where found matching with {debts.Cuit} Cuit number.");

                    var bu = userBU.FirstOrDefault()?.BusinessUnit;
                    if (bu != null)
                    {
                        if (!usuariosPorBusinessUnit.ContainsKey(bu))
                        {
                            usuariosPorBusinessUnit[bu] = new List<User>();
                        }
                        usuariosPorBusinessUnit[bu].Add(user);
                    }
                }

                foreach (var kvp in usuariosPorBusinessUnit)
                {
                    var bu = kvp.Key;
                    var usuarios = kvp.Value;

                    if (usuarios.Any())
                    {
                        var userTable = GenerateUserTable(usuarios);
                        string templateConTabla = htmlTemplate.Replace("{{tabla_placeholder}}", userTable);

                        // Busca el destinatario correspondiente en la configuración y envía el correo
                        var recipient = recipients.FirstOrDefault(r => r.BusinessUnit == bu);
                        if (recipient != null)
                        {
                            _mailService.SendNotificationEmail(recipient.Email, $"Aviso de libre deuda para {bu}", templateConTabla);
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Log.Error(@"Error in sending free debt user's report: {@err}", ex);
            }
        }
        private string GenerateUserTable(List<User> users)
        {
            var tableBuilder = new StringBuilder();

            tableBuilder.AppendLine("<table>");
            tableBuilder.AppendLine("<tr>");
            tableBuilder.AppendLine("<th>Cuit</th></br>");
            tableBuilder.AppendLine("<th>Nombre y Apellido</th>");
            tableBuilder.AppendLine("<th>Email</th>");
            tableBuilder.AppendLine("</tr>");

            foreach (var user in users)
            {
                tableBuilder.AppendLine("<tr>");
                tableBuilder.AppendLine($"<td>{user.Cuit}</td>");
                tableBuilder.AppendLine($"<td>{user.FirstName} {user.LastName}</td>");
                tableBuilder.AppendLine($"<td>{user.Email}</td>");
                tableBuilder.AppendLine("</tr>");
            }

            tableBuilder.AppendLine("</table>");
            return tableBuilder.ToString();
        }

    }
}
