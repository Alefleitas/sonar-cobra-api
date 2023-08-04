using cobra.service.mail.listener.communications.Configuration;
using cobra.service.mail.listener.communications.Models;
using cobra.service.mail.listener.communications.Utils;
using MailKit;
using MailKit.Net.Imap;
using Microsoft.Extensions.Options;
using MimeKit;
using System.Text.RegularExpressions;
using MimeKit.Text;
using AngleSharp;
using IConfiguration = Microsoft.Extensions.Configuration.IConfiguration;
using MailKit.Security;
using Newtonsoft.Json.Linq;
using Serilog;

namespace cobra.service.mail.listener.communications.Services
{
    internal class EmailListenerService : EmailMessage, IEmailListenerService
    {
        private readonly int _maxRetryNumber;
        private readonly ILogger<EmailListenerService> _emailListenerLogger;
        private readonly IOptions<EmailSenderConfig> _emailSenderConfig;
        private readonly ICommunicationService _communicationService;
        private readonly EmailListenerConfig _emailListenerImap;
        private readonly AzureAdCredentialConfig _azureAdCredentialConfig;

        public EmailListenerService(
            IConfiguration configuration, 
            ILogger<EmailListenerService> logger, 
            IOptions<EmailSenderConfig> emailSenderConfig,
            ICommunicationService communicationService,
            IOptionsMonitor<EmailListenerConfig> emailListenerConfig,
            IOptionsMonitor<AzureAdCredentialConfig> azureAdCredentialConfig
            )
        {
            _maxRetryNumber = configuration.GetValue<int>("BotMaxRetry");
            _emailListenerLogger = logger;
            _emailSenderConfig = emailSenderConfig;
            _communicationService = communicationService;
            _emailListenerImap = emailListenerConfig.Get(EmailListenerConfig.EmailListenerImap);
            _azureAdCredentialConfig = azureAdCredentialConfig.Get(AzureAdCredentialConfig.AzureAdEmailListener);
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
                    var token = json["access_token"]!;

                    return new SaslMechanismOAuth2(_azureAdCredentialConfig.Email, token.ToString());
                }
            }
            catch (Exception ex)
            {
                Log.Error(ex, "EmailListenerService.ListenForIncoming(): Ocurrió un error al intentar obtener el token de acceso");
                throw;
            }
        }

        public async Task ListenForIncomingAsync(DateTime? dateTime, int retryNumber = 0)
        {
            dateTime ??= LocalDateTime.GetDateTimeNow();
            _emailListenerLogger.LogInformation("EmailListenerService.ListenForIncoming() Started.");

            using (var waitForEmailIncoming = new AutoResetEvent(false))
            using (var client = new ImapClient())
            {
                try
                {
                    client.Timeout = (int)TimeSpan.FromMinutes(5).TotalMilliseconds;

                    await client.ConnectAsync(_emailListenerImap.Host, _emailListenerImap.Port, SecureSocketOptions.SslOnConnect);
                    var oauth2 = await GetAccessTokenAsync(_emailListenerImap.Scopes);
                    await client.AuthenticateAsync(oauth2);

                    client.Inbox.Open(FolderAccess.ReadOnly);

                    int oldCount = client.Inbox.Count, newCount = client.Inbox.Count;

                    client.Inbox.CountChanged += (sender, e) =>
                    {
                        var folder = (ImapFolder)sender!;

                        if (folder.Count > newCount)
                        {
                            _emailListenerLogger.LogInformation("EmailListenerService.ListenForIncoming() Nuevo correo en la casilla");
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
                        _emailListenerLogger.LogInformation($"EmailListenerService.ListenForIncoming() Se Borro un correo de la casilla");
                        newCount--;
                    };

                    while (true)
                    {
                        #region Escuchar mensajes
                        using (var done = new CancellationTokenSource())
                        {
                            _emailListenerLogger.LogInformation("EmailListenerService.ListenForIncoming() Starts IMAP IDLE");

                            var task = client.IdleAsync(done.Token);

                            waitForEmailIncoming.WaitOne(20 * 60 * 1000); // Tiempo de espera de 20 minutos
                            done.Cancel();
                            task.Wait();

                            _emailListenerLogger.LogInformation("EmailListenerService.ListenForIncoming() Canceled IMAP IDLE");
                        }
                        #endregion

                        #region Procesar mensajes
                        if (newCount > oldCount)
                        {
                            var communications = new List<Communication>();
                            var messages = await client.Inbox.FetchAsync(oldCount, -1, MessageSummaryItems.Full | MessageSummaryItems.UniqueId | MessageSummaryItems.BodyStructure);
                            oldCount = newCount;
                            _emailListenerLogger.LogInformation("EmailListenerService.ListenForIncoming() {count} new emails arrived.", messages.Count);

                            foreach (var email in messages)
                            {
                                try
                                {
                                    var message = await client.Inbox.GetMessageAsync(email.UniqueId);

                                    var senders = message.From.OfType<MailboxAddress>().ToList();
                                    if (senders.Count > 1)
                                    {
                                        _emailListenerLogger.LogWarning("Error more than one sender. Count: {@senders}", senders);
                                        continue;
                                    }
                                    var sender = senders.Single().Address;

                                    var validDomains = _emailSenderConfig.Value.ValidDomains;
                                    if (!validDomains.Any(x => sender.EndsWith(x)))
                                    {
                                        _emailListenerLogger.LogWarning("Communication, email sender needs to be valid. Sender: {sender}", sender);
                                        continue;
                                    }
                                    await ProcessNewEmail(message, sender, communications);
                                }
                                catch (Exception ex)
                                {
                                    _emailListenerLogger.LogError(ex, "EmailListenerService.ListenForIncoming(): Ocurrió un error al procesar mensaje: {@msg}", email);
                                }
                            }

                            if (communications.Any())
                            {
                                await _communicationService.PostCommunication(communications);
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
                    _emailListenerLogger.LogError(ex, "Error in EmailListenerService.ListenForIncoming()");
                    await client.DisconnectAsync(true);

                    // Si volvemos a intentarlo con una fecha diferente, reiniciamos el retryCount

                    var dateNow = LocalDateTime.GetDateTimeNow();

                    if (dateNow.Date > dateTime.Value.Date)
                    {
                        _emailListenerLogger.LogInformation(ex, "EmailListenerService.ListenForIncoming() Retry count reseted ({retry}).", retryNumber);
                        retryNumber = 0;
                    }
                    if (retryNumber < _maxRetryNumber)
                    {
                        _emailListenerLogger.LogWarning(ex, "EmailListenerService.ListenForIncoming() Retry count error ({retry}).", retryNumber);
                        await ListenForIncomingAsync(dateNow, retryNumber + 1);
                    }
                    else
                    {
                        _emailListenerLogger.LogError(ex, "EmailListenerService.ListenForIncoming() Retry count error, the maximum number of retries has been reached; ({max})", _maxRetryNumber);
                        throw;
                    }
                }
            }
        }

        private async Task ProcessNewEmail(MimeMessage message, string sender, ICollection<Communication> communications)
        {
            const string pattern = @"\(([^()]{1,25})\)";
            try
            {
                if (Regex.Match(message.Subject, pattern).Success)
                {
                    var productCode = Regex.Match(message.Subject, pattern).Groups[1].Value;
                    _emailListenerLogger.LogInformation("Subject: {subject}", message.Subject);

                    var bodyContent = await GetBodyMessage(message);

                    var communication = new Communication
                    {
                        Product = productCode,
                        Date = message.Date.UtcDateTime,
                        MessageId = message.MessageId,
                        Sender = sender,
                        Subject = message.Subject,
                        Body = bodyContent
                    };

                    communications.Add(communication);
                    _emailListenerLogger.LogInformation("Communication: {@comm}", communication);
                }
                else
                {
                    _emailListenerLogger.LogWarning("Invalid Subject for new Communication: {subject}", message.Subject);

                    // POST to Cobra API with product codes as null

                    var productNullEmail = new Communication
                    {
                        Product = null,
                        MessageId = message.MessageId,
                        Sender = sender
                    };

                    communications.Add(productNullEmail);
                    _emailListenerLogger.LogInformation("Invalid Communication: {@comm}", productNullEmail);
                }
            }
            catch (Exception ex)
            {
                _emailListenerLogger.LogError(ex, "Error on CreateCommunicationFromService");
                throw;
            }
        }

        private async Task<string> GetBodyMessage(MimeMessage message)
        {
            string bodyContent = "";

            // Try get TextBody
            var textBody = message.GetTextBody(TextFormat.Plain);

            if (textBody != null)
            {
                _emailListenerLogger.LogInformation("Communication: Successfully get plain-text from body.");
                bodyContent = CleanupTextPlainBody(textBody);
            }
            else
            {
                bodyContent = await ParseHtmlBody(message);
            }

            var mailArrobado = "@" + _azureAdCredentialConfig.Email;
            if (bodyContent.Contains(mailArrobado))
            {
                bodyContent = bodyContent.Replace(mailArrobado + $"<mailto:{_azureAdCredentialConfig.Email}>", String.Empty);
            }

            return bodyContent;
        }

        private async Task<string> ParseHtmlBody(MimeMessage message)
        {
            string parsedBody = "";
            var angleSharpConfig = AngleSharp.Configuration.Default;
            var htmlBody = message.HtmlBody;
            if (string.IsNullOrEmpty(htmlBody))
            {
                _emailListenerLogger.LogWarning("Communication: message htmlBody cannot be null. Message ContentType: {@body}",
                    message.Body.ContentType);
                return parsedBody;
            }
            try
            {
                _emailListenerLogger.LogInformation("Communication: Trying to get fullBody Html content.");

                using (var angleSharpContext = new BrowsingContext(angleSharpConfig))
                {
                    var document = await angleSharpContext.OpenAsync(req => req.Content(htmlBody));

                    var onlyBody = document.GetElementsByTagName("body").FirstOrDefault();
                    if (onlyBody is null)
                    {
                        _emailListenerLogger.LogWarning("Communication: Cannot get body from html: {@body}",
                            document.Body);
                        return parsedBody;
                    }

                    var divElements = onlyBody.GetElementsByTagName("div");
                    if (!divElements.Any())
                    {
                        _emailListenerLogger.LogWarning("Communication: not found any div in the html body: {@body}",
                            document.Body);
                        parsedBody = onlyBody.TextContent;
                    }
                    else
                    {
                        var htmlBodyParts = new List<string>();
                        foreach (var div in divElements)
                        {
                            htmlBodyParts.Add(div.TextContent);
                        }
                        parsedBody = CleanupHtmlBody(htmlBodyParts);
                    }
                }
            }
            catch (Exception ex)
            {
                _emailListenerLogger.LogError(ex, "Communication: fullBody cannot be null. Message: {@body}",
                    message.Body.ContentType);
            }
            return parsedBody;
        }
        private static string CleanupTextPlainBody(string textBody)
        {
            var indexOfTrail = textBody.IndexOf("________________________________", StringComparison.Ordinal);

            if (indexOfTrail < 0)
            {
                indexOfTrail = textBody.IndexOf("\r\n\r\nDe: ", StringComparison.Ordinal);
            }
            if (indexOfTrail < 0)
            {
                indexOfTrail = textBody.IndexOf("\r\n\r\nFrom: ", StringComparison.Ordinal);
            }

            return indexOfTrail < 0 ? textBody : textBody[..indexOfTrail];
        }
        private string CleanupHtmlBody(List<string> htmlBodyParts)
        {
            var htmlText = "";
            var datePattern = @"El\s\d{1,2}\/\d{1,2}\/\d{2,4}";


            var repliedEmailIndex = htmlBodyParts.FindIndex(x =>
                x.Contains("De:") || x.Contains("From:") || Regex.Match(x, datePattern).Success);

            if (repliedEmailIndex < 0)
            {
                _emailListenerLogger.LogWarning(
                    "Communication: Cannot get repliedEmail from html body: {@body}",
                    htmlBodyParts);
            }
            else
            {
                if (repliedEmailIndex == 0)
                {
                    ObtenerRegistroDeCadena(htmlBodyParts);
                }
                repliedEmailIndex += 1;
                htmlBodyParts.RemoveRange(repliedEmailIndex, (htmlBodyParts.Count - repliedEmailIndex));
            }

            var lastEmailContent = string.Join(" ", htmlBodyParts);

            return lastEmailContent.Replace("\r", string.Empty).Replace("\n", string.Empty);
        }

        protected void ObtenerRegistroDeCadena(List<string> htmlBodyParts)
        {

            string[] result = Regex.Split(htmlBodyParts[0].Trim(), @"(?=De:|From:)");

            var registro = result.FirstOrDefault(x => !string.IsNullOrEmpty(x));

            htmlBodyParts.Insert(0, registro);

        }

    }
}






