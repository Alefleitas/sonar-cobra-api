using nordelta.cobra.webapi.Configuration;
using nordelta.cobra.webapi.Controllers.ViewModels;
using System.Net.Mail;
using System.Net;
using System.Text;
using Microsoft.Extensions.Options;
using nordelta.cobra.webapi.Models;
using System.Linq;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Serilog;
using Microsoft.AspNetCore.Http;
using System.IO;
using nordelta.cobra.webapi.Models.MessageChannel;
using nordelta.cobra.webapi.Models.MessageChannel.Messages;
using MailKit.Net.Imap;
using MailKit;
using System.Threading;
using MailKit.Security;
using MimeKit;
using SmtpClient = System.Net.Mail.SmtpClient;
using Newtonsoft.Json;
using Microsoft.Extensions.Configuration;
using nordelta.cobra.webapi.Utils;
using Hangfire;
using Newtonsoft.Json.Linq;
using System.Net.Http;

namespace nordelta.cobra.webapi.Services
{
    public class MailService : Contracts.IMailService, IMessageChannel<EmailMessage>, IMessageChannel<IMessage>
    {
        readonly EmailConfig _emailConfig;
        readonly EmailConfig _smtpQuotationBotConfig;
        readonly EmailConfig _imapQuotationBotConfig;
        readonly SmtpClient smtpClient;
        readonly AzureAdCredentialConfig _azureAdCredentialConfig;

        IConfiguration _configuration;
        private readonly int _maxRetryNumber;
        private readonly IEnumerable<string> _recipientsEmailQuotationBot;
        private List<IMessageChannelObserver> IncomingObservers = new List<IMessageChannelObserver>();

        public MailService(
            IOptionsMonitor<EmailConfig> emailConfig,
            IOptionsMonitor<AzureAdCredentialConfig> azureAdCredentialConfig,
            IConfiguration configuration
            )
        {
            _emailConfig = emailConfig.Get(EmailConfig.EmailSmtpConfig);
            _smtpQuotationBotConfig = emailConfig.Get(EmailConfig.EmailSmtpQuotationBotConfig);
            _imapQuotationBotConfig = emailConfig.Get(EmailConfig.EmailImapQuotationBotConfig);
            _azureAdCredentialConfig = azureAdCredentialConfig.Get(AzureAdCredentialConfig.AzureAdQuotationBot);

            _configuration = configuration;
            _maxRetryNumber = configuration.GetValue<int>("QuotationBotMaxRetry");
            _recipientsEmailQuotationBot = configuration.GetSection("ServiceConfiguration:RecipientsEmailQuotationBot").Get<IEnumerable<string>>();

            if (_emailConfig != null)
            {
                smtpClient = new SmtpClient
                {
                    Host = _emailConfig.Host,
                    Port = _emailConfig.Port,
                    EnableSsl = _emailConfig.EnableSsl,
                    Timeout = _emailConfig.TimeoutInMinutes * 60000,
                    DeliveryMethod = SmtpDeliveryMethod.Network,
                    UseDefaultCredentials = false,
                    Credentials = new NetworkCredential(_emailConfig.Email, _emailConfig.Password)
                };
            }
        }

        public Task SendRepeatedLegalEmail(IEnumerable<DepartmentChangeNotification> details, IEnumerable<string> recipients)
        {
            var receiver = string.Join(",", recipients);
            try
            {
                var builder = new StringBuilder();
                builder.Append(@"<style>
                table, th, td {
                  border: 1px solid black;
                }
                tr:nth-child(even) {
                  background-color: #f2f2f2;
                }
                table {
                    height: 100%;
                }
                </style>");
                builder.Append("<div><table><tr>");
                builder.Append("<th>RazonSocial</th>");
                builder.Append("<th>ClienteCuit</th>");
                builder.Append("<th>CodigoProducto</th>");
                builder.Append("</tr>");
                foreach (var debtDetail in details)
                {
                    builder.Append("<tr><td>");
                    builder.Append(debtDetail.RazonSocial);
                    builder.Append("</td><td>");
                    builder.Append(debtDetail.NumeroCuitCliente);
                    builder.Append("</td><td>");
                    builder.Append(debtDetail.CodigoProducto);
                    builder.Append("</td></tr>");
                }
                builder.Append("</table></div>");

                MailMessage mailMessage = new MailMessage(_emailConfig.Email, receiver)
                {
                    Subject = "COBRA - Cambio de Estado a Legales",
                    Body = builder.ToString(),
                    IsBodyHtml = true
                };
                string ccEmail = _configuration.GetSection("ServiceConfiguration:SupportITEmailCC").Value;
                string ccoEmail = _configuration.GetSection("ServiceConfiguration:SupportITEmailCCO").Value;
                if (!string.IsNullOrEmpty(ccEmail))
                {
                    mailMessage.CC.Add(ccEmail);
                }
                if (!string.IsNullOrEmpty(ccoEmail))
                {
                    mailMessage.Bcc.Add(ccoEmail);
                }
                using (MailMessage messageToSend = mailMessage)
                {
                    this.smtpClient.Send(messageToSend);
                }
            }
            catch (Exception ex)
            {
                Log.Error(ex, "No se puede enviar email de legalesNotifications a: {0}, Ex: {1}", receiver, ex);
                throw;
            }
            Log.Debug("Se envío un email de legalesNotifications a: {0}", receiver);
            return Task.CompletedTask;
        }
        
        public Task SendRepeatedDebtDetailsEmail(IEnumerable<RepeatedDebtDetail> details, IEnumerable<string> recipients)
        {
            var receiver = string.Join(",", recipients);
            try
            {
                var builder = new StringBuilder();
                builder.Append(@"<style>
                table, th, td {
                  border: 1px solid black;
                }
                tr:nth-child(even) {
                  background-color: #f2f2f2;
                }
                table {
                    height: 100%;
                }
                </style>");
                builder.Append("<div><table><tr>");
                builder.Append("<th>NombreArchivo</th>");
                builder.Append("<th>NroComprobante</th>");
                builder.Append("<th>FechaPrimerVenc</th>");
                builder.Append("<th>CodigoMoneda</th>");
                builder.Append("<th>CodigoProducto</th>");
                builder.Append("<th>CodigoTransaccion</th>");
                builder.Append("<th>RazonSocialCliente</th>");
                builder.Append("<th>NroCuitCliente</th>");
                builder.Append("<th>IdClienteOracle</th>");
                builder.Append("<th>IdSiteOracle</th>");
                builder.Append("</tr>");
                foreach (var debtDetail in details)
                {
                    builder.Append("<tr><td>");
                    builder.Append(debtDetail.FileName);
                    builder.Append("</td><td>");
                    builder.Append(debtDetail.NroComprobante);
                    builder.Append("</td><td>");
                    builder.Append(debtDetail.FechaPrimerVenc);
                    builder.Append("</td><td>");
                    builder.Append(debtDetail.CodigoMoneda);
                    builder.Append("</td><td>");
                    builder.Append(debtDetail.CodigoProducto);
                    builder.Append("</td><td>");
                    builder.Append(debtDetail.CodigoTransaccion);
                    builder.Append("</td><td>");
                    builder.Append(debtDetail.RazonSocialCliente);
                    builder.Append("</td><td>");
                    builder.Append(debtDetail.NroCuitCliente);
                    builder.Append("</td><td>");
                    builder.Append(debtDetail.IdClienteOracle);
                    builder.Append("</td><td>");
                    builder.Append(debtDetail.IdSiteOracle);
                    builder.Append("</td></tr>");
                }
                builder.Append("</table></div>");

                MailMessage mailMessage = new MailMessage(this._emailConfig.Email, receiver)
                {
                    Subject = "COBRA - Detalles de Deuda Repetidos",
                    Body = builder.ToString(),
                    IsBodyHtml = true
                };
                string ccEmail = _configuration.GetSection("ServiceConfiguration:SupportITEmailCC").Value;
                string ccoEmail = _configuration.GetSection("ServiceConfiguration:SupportITEmailCCO").Value;
                if (!string.IsNullOrEmpty(ccEmail))
                {
                    mailMessage.CC.Add(ccEmail);
                }
                if (!string.IsNullOrEmpty(ccoEmail))
                {
                    mailMessage.Bcc.Add(ccoEmail);
                }
                using (MailMessage messageToSend = mailMessage)
                {
                    this.smtpClient.Send(messageToSend);
                }
            }
            catch (Exception ex)
            {
                Log.Error(ex, "No se puede enviar email de detallesDeuda repetidos a: {0}, Ex: {1}", receiver, ex);
                throw;
            }
            Log.Debug("Se envío un email de detallesDeuda repetidos a: {0}", receiver);
            return Task.CompletedTask;
        }

        public bool sendContactEmail(ContactViewModel contactForm, IList<IFormFile> attachments)
        {
            StringBuilder sb = new StringBuilder();

            var contactEmailTo = GetContactsMails().FirstOrDefault(x => x.Key == contactForm.Cuit).Value;

            if (contactEmailTo is null)
            {
                Log.Error($"sendContactEmail: Error sending cuit: {contactForm.Cuit} not found");
                throw new Exception($"sendContactEmail: Error sending cuit: {contactForm.Cuit} not found");
            }

            sb.Append("Cliente: ");
            sb.Append(contactForm.Name);
            sb.Append("\n");
            sb.Append("Email: ");
            sb.Append(contactForm.Email);
            sb.Append("\n");
            sb.Append("Teléfono: ");
            sb.Append(contactForm.Tel);
            sb.Append("\n");
            sb.Append("Producto: ");
            sb.Append(contactForm.Product);
            sb.Append("\n\n");
            sb.Append("Mensaje: ");
            sb.Append(contactForm.Message);
            sb.Append("\n");


            using (var message =
                new MailMessage(_emailConfig.Email, contactEmailTo)
                {
                    Subject = "COBRA - Nuevo contacto",
                    Body = sb.ToString()
                })
            {
                var msList = new List<MemoryStream>();
                foreach (var file in attachments)
                {
                    var ms = new MemoryStream();
                    msList.Add(ms);
                    file.CopyTo(ms);
                    ms.Position = 0;
                    var attachment = new Attachment(ms, file.FileName);
                    message.Attachments.Add(attachment);
                }

                this.smtpClient.Send(message);

                foreach (var ms in msList)
                {
                    ms.Dispose();
                }
                return true;
            }

        }

        public void SendNotificationEmail(string email, string subject, string body)
        {
            try
            {
                smtpClient.DeliveryMethod = SmtpDeliveryMethod.Network;

                MailMessage mailMessage = new MailMessage();
                mailMessage.From = new MailAddress(_emailConfig.Email);
                mailMessage.Subject = subject;

                mailMessage.To.Add(email);
                mailMessage.Body = @body;
                mailMessage.IsBodyHtml = true;

                using (MailMessage messageToSend = mailMessage)
                    this.smtpClient.Send(messageToSend);
            }
            catch (Exception e)
            {
                Log.Error("SendNotificationEmailError: Error sending notification emails: @{e}", e);
            }
        }

        [Queue("default")]
        [JobDisplayName("SendNotificationEmail")]
        [AutomaticRetry(Attempts = 10, OnAttemptsExceeded = AttemptsExceededAction.Fail)]
        public void SendNotificationEmail(List<string> emails, string subject, string body)
        {
            try
            {
                if (emails.Any())
                {
                    MailMessage mailMessage = new MailMessage();
                    mailMessage.From = new MailAddress(_emailConfig.Email);
                    mailMessage.Subject = subject;

                    foreach (var email in emails)
                        mailMessage.To.Add(email);
                    mailMessage.Body = @body;
                    mailMessage.IsBodyHtml = true;

                    using (MailMessage messageToSend = mailMessage)
                    {
                        this.smtpClient.Send(messageToSend);
                    }
                }
            }
            catch (Exception ex)
            {
                Log.Error(ex,"SendNotificationEmailError: Error sending notification emails");
                throw;
            }
        }

        public void SendEmailQuotationBot(string details, IEnumerable<string> recipients = null, Exception ex = null)
        {
            var receiver = string.Join(",", recipients ?? _recipientsEmailQuotationBot);

            try
            {
                var builder = new StringBuilder();

                builder.Append(details);

                if (ex != null)
                {
                    builder.Append("<br>");
                    builder.Append("Exception: " + ex.Message);
                }

                MailMessage mailMessage = new MailMessage(this._emailConfig.Email, receiver)
                {
                    Subject = "COBRA - Quotation Bot",
                    Body = builder.ToString(),
                    IsBodyHtml = true
                };
                using (MailMessage messageToSend = mailMessage)
                {
                    this.smtpClient.Send(messageToSend);
                }
            }
            catch (Exception e)
            {
                Log.Error(e, "No se puedo enviar email de QuotationBot a: {0}, Ex: {1}", receiver, e);
                throw;
            }
            Log.Debug("Se envío un email de QuotationBot a: {0}", receiver);
        }

        private IDictionary<string, string> GetContactsMails()
        {
            return _configuration.GetSection("ServiceConfiguration:EmailContacts").GetChildren().ToDictionary(x => x.Key, x => x.Value);
        }

        public virtual void SubscribeForIncoming(IMessageChannelObserver observer)
        {
            this.IncomingObservers.Add(observer);
            Log.Debug($"{observer} se ha subscripto como observer de MailService (IMessageChannel de tipo Email). Hay {this.IncomingObservers.Count} observers en total");
        }

        private async Task<SaslMechanismOAuth2> GetAccessTokenAsync(string[] scopes)
        {
            try
            {
                var scopesStr = string.Join(" ", scopes.Select(x => x?.Trim()).Where(x => !string.IsNullOrEmpty(x)));
                var content = new FormUrlEncodedContent(new List<KeyValuePair<string, string>>
                {
                    new KeyValuePair<string, string>("grant_type", "password"),
                    new KeyValuePair<string, string>("username", _azureAdCredentialConfig.Email),
                    new KeyValuePair<string, string>("password", _azureAdCredentialConfig.Password),
                    new KeyValuePair<string, string>("client_id", _azureAdCredentialConfig.ClientId),
                    new KeyValuePair<string, string>("client_secret", _azureAdCredentialConfig.ClientSecret),
                    new KeyValuePair<string, string>("scope", scopesStr),
                });

                using (var client = new HttpClient())
                {
                    var response = await client.PostAsync(_azureAdCredentialConfig.RequesUri, content).ConfigureAwait(continueOnCapturedContext: false);
                    var responseString = await response.Content.ReadAsStringAsync();
                    var json = JObject.Parse(responseString);
                    var token = json["access_token"];

                    return token != null ? new SaslMechanismOAuth2(_azureAdCredentialConfig.Email, token.ToString()) : null;
                }
            }
            catch (Exception ex)
            {
                Log.Error(ex, "MailService.GetAccessTokenAsync(): Ocurrió un error al intentar obtener token de acceso");
                throw;
            }
        }

        public async virtual Task ListenForIncomingAsync(DateTime? dateTime, int retryNumber = 0)
        {
            if (!dateTime.HasValue) dateTime = LocalDateTime.GetDateTimeNow();

            Log.Debug($"MailService.ListenForIncoming() Start");

            using (var waitForEmailIncoming = new AutoResetEvent(false))
            using (var client = new ImapClient())
            {
                try
                {
                    await client.ConnectAsync(_imapQuotationBotConfig.Host, _imapQuotationBotConfig.Port, SecureSocketOptions.SslOnConnect);
                    var oauth2 = await GetAccessTokenAsync(_imapQuotationBotConfig.Scopes);
                    await client.AuthenticateAsync(oauth2);

                    client.Inbox.Open(FolderAccess.ReadOnly);

                    int oldCount = client.Inbox.Count, newCount = client.Inbox.Count;

                    client.Inbox.CountChanged += (sender, e) =>
                    {
                        var folder = (ImapFolder)sender;

                        if (folder.Count > newCount)
                        {
                            Log.Debug("MailService.ListenForIncoming Nuevo correo en la casilla");
                            newCount = folder.Count;
                            waitForEmailIncoming.Set();
                        }
                        else
                        {
                            newCount = folder.Count;
                        }
                    };

                    client.Inbox.MessageExpunged += (sender, e) =>
                    {
                        Log.Debug($"MailService.ListenForIncoming Se Borro un correo de la casilla");
                        newCount--;
                    };

                    while (true)
                    {
                        #region Escuchar mensajes
                        using (var done = new CancellationTokenSource())
                        {
                            Log.Debug("MailService.ListenForIncoming() Starts IMAP IDLE");

                            var task = client.IdleAsync(done.Token);

                            waitForEmailIncoming.WaitOne(20 * 60 * 1000); // Tiempo de espera de 20 minutos
                            done.Cancel();
                            task.Wait();

                            Log.Debug("MailService.ListenForIncoming() Canceled IMAP IDLE");
                        }
                        #endregion

                        #region Procesar mensajes
                        if (newCount > oldCount)
                        {
                            var messages = client.Inbox.Fetch(oldCount, -1, MessageSummaryItems.Full | MessageSummaryItems.UniqueId);
                            oldCount = newCount;

                            Log.Information($"MailService.ListenForIncoming() Se obtienen los {messages.Count} correos nuevos y se notifica asincronicamente a los {IncomingObservers.Count} observers.");

                            foreach (var msg in messages)
                            {
                                try
                                {
                                    var emailMessage = new EmailMessage()
                                    {
                                        Body = msg.Body.ToString(),
                                        Subject = msg.Envelope.Subject,
                                        DateTimeSent = msg.Envelope.Date.Value.DateTime,
                                        To = msg.Envelope.To.Mailboxes.FirstOrDefault()?.Address,
                                        From = msg.Envelope.From.Mailboxes.FirstOrDefault()?.Address
                                    };

                                    foreach (var item in IncomingObservers)
                                    {
                                        await item.NotifyIncomingMessageAsync(emailMessage, this);
                                    }
                                }
                                catch (Exception ex)
                                {
                                    Log.Error(ex, $"MailService.ListenForIncoming() Falla al notificar observers asincronamente. Usando _quotationBotConfig: {JsonConvert.SerializeObject(_imapQuotationBotConfig)}");
                                    SendEmailQuotationBot("MailService.ListenForIncoming() Falla al notificar observers asincronamente", ex: ex);
                                }
                            }
                        }
                        else if (newCount < oldCount)
                        {
                            oldCount = newCount = client.Inbox.Count;
                        }
                        #endregion
                    }
                }
                catch (Exception ex)
                {
                    await client.DisconnectAsync(true);

                    //Si volvemos a intentarlo en una fecha diferente, reiniciamos el retryCount
                    var dateNow = LocalDateTime.GetDateTimeNow();

                    if (dateNow.Date > dateTime.Value.Date)
                    {
                        Log.Warning(ex, $"MailService.ListenForIncoming() se resetea el retry count ({retryNumber})");
                        retryNumber = 0;
                    }
                    if (retryNumber < _maxRetryNumber)
                    {
                        Log.Warning(ex, $"MailService.ListenForIncoming() falla con quotationBot retry count ({retryNumber})");
                        await ListenForIncomingAsync(dateNow, retryNumber + 1);
                    }
                    else
                    {
                        Log.Error(ex, $"MailService.ListenForIncoming() falla con quotationBot se ah alcanzado la cantidad maxima de reintentos ({_maxRetryNumber})");
                        SendEmailQuotationBot($"MailService.ListenForIncoming() falla con quotationBot se ah alcanzado la cantidad maxima de reintentos ({_maxRetryNumber})", _recipientsEmailQuotationBot, ex);
                        throw;
                    }
                }
            }
        }

        public virtual async Task SendMessageAsync(IMessage message)
        {
            Log.Debug($"MailService.SendMessage() se ejecuta con mensaje: {JsonConvert.SerializeObject(message)}");
            await SendMessageAsync((EmailMessage)message);
        }

        public async Task SendMessageAsync(EmailMessage message)
        {
            try
            {
                using (var client = new MailKit.Net.Smtp.SmtpClient())
                {
                    client.CheckCertificateRevocation = false;

                    await client.ConnectAsync(_smtpQuotationBotConfig.Host, _smtpQuotationBotConfig.Port, SecureSocketOptions.StartTls);
                    var oauth2 = await GetAccessTokenAsync(_smtpQuotationBotConfig.Scopes);
                    await client.AuthenticateAsync(oauth2);

                    var msg = new MimeMessage();
                    msg.From.Add(MailboxAddress.Parse(message.From));
                    msg.To.Add(MailboxAddress.Parse(message.To));
                    msg.Subject = message.Subject;
                    msg.Body = new TextPart("html") { Text = message.Body };

                    await client.SendAsync(msg);
                    await client.DisconnectAsync(true);
                }
            }
            catch (Exception ex)
            {
                Log.Error(ex, "MailService.SendMessageAsync(): Ocurrío un error al intentar enviar un mensaje: {msg}", JsonConvert.SerializeObject(message));
                throw;
            }
        }

    }
}
