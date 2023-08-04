using Newtonsoft.Json;
using nordelta.cobra.webapi.Models.MessageChannel;
using nordelta.cobra.webapi.Models.MessageChannel.Messages;
using nordelta.cobra.webapi.Repositories.Contracts;
using nordelta.cobra.webapi.Services.Contracts;
using Serilog;
using System;
using System.Collections.Generic;
using Microsoft.Extensions.Configuration;
using Microsoft.VisualBasic;
using nordelta.cobra.webapi.Configuration;
using nordelta.cobra.webapi.Models;
using nordelta.cobra.webapi.Utils;
using Monitoreo = Nordelta.Monitoreo;
using Microsoft.Extensions.Options;
using System.Threading.Tasks;

namespace nordelta.cobra.webapi.Services
{
    public class QuotationBotService : IQuotationBotService, IMessageChannelObserver
    {
        private List<IMessageChannel<IMessage>> MessageChannels;
        private IExchangeRateFileRepository exchangeRateFileRepository;
        private readonly INotificationRepository notificationRepository;
        private readonly List<string> quotationBotRateTypes;
        private readonly ServiciosMonitoreadosConfiguration _servicios;
        public QuotationBotService(IExchangeRateFileRepository exchangeRateFileRepository, IMessageChannel<EmailMessage> emailChannel, INotificationRepository notificationRepository, IConfiguration configuration, IOptions<ServiciosMonitoreadosConfiguration> servicesMonConfig)
        {
            this.MessageChannels = new List<IMessageChannel<IMessage>>();
            this.MessageChannels.Add((IMessageChannel<IMessage>)emailChannel);
            this.exchangeRateFileRepository = exchangeRateFileRepository;
            this.notificationRepository = notificationRepository;
            this.quotationBotRateTypes = configuration.GetSection("QuotationBotRateTypes").Get<List<string>>();
            _servicios = servicesMonConfig.Value;
        }

        public async Task ListenAllChannelsAsync()
        {
            Log.Debug($"QuotationBot empieza a escuchar en todos los {this.MessageChannels.Count} canales");

            try
            {
                foreach (var item in MessageChannels)
                {
                    item.SubscribeForIncoming(this);
                    await item.ListenForIncomingAsync(null);
                }

                Monitoreo.Monitor.Ok($"QuotationBot empieza a escuchar en todos los {this.MessageChannels.Count} canales", _servicios.TCMail);
            }
            catch (Exception ex)
            {
                Log.Error(ex, "QuotationBotService.ListenAllChannels() alcanzo la cantidad maxima de reintentos");
                Monitoreo.Monitor.Critical("QuotationBotService.ListenAllChannels() alcanzó la cantidad maxima de reintentos", _servicios.TCMail);
            }
        }

        public async Task NotifyIncomingMessageAsync(IMessage message, IMessageChannel<IMessage> messageChannel)
        {
            try
            {
                var quotations = new List<dynamic>();

                foreach (var rateType in quotationBotRateTypes)
                {
                    try
                    {
                        var quotation = this.exchangeRateFileRepository.GetCurrentQuotation(rateType);
                        if (quotation != null)
                        {
                            quotations.Add(quotation);
                        }
                    }
                    catch (Exception ex)
                    {
                        Log.Error(ex, "QuotationBotService.NotifyIncomingMessage(): Ocurrió un error al momento de obtener la cotización {rate}", rateType);
                        Monitoreo.Monitor.Warning($"QuotationBotService.NotifyIncomingMessage: error al momento de obtener la cotización {rateType}", _servicios.TCMail);
                    }
                }

                var htmlBody = this.notificationRepository.GetTemplateByDescInternal(TemplateDescription.Quotations).HtmlBody;
                var table = String.Empty;

                try
                {
                    foreach (var quotation in quotations)
                    {
                        if (quotation.Valor == 0)
                        {
                            //messageChannel.SendEmailQuotationBot($"Cotizacion {quotation.RateType} con valor 0");
                        }
                        else
                        {
                            table +=
                                $"<div id=\"quotations\"><table><tbody><tr><td><div class=\"mj-column-per-50 outlook-group-fix\" id=\"quotColumn\">" +
                                $@"<table><tbody><tr><td><div><p><span>
                          {(quotation.RateType == RateTypes.Cac ? quotation.FromCurrency.ToString() + "-" +
                                                                      quotation.ToCurrency.ToString() : quotation.RateType.ToString())} {(quotation.RateType == RateTypes.UsdMEP ? $"(Según {quotation.Especie.ToString()})" :
                                                                        quotation.Source.ToString())}
                            </span></p></div></td></tr></tbody></table>"" +
                            $""</div><div class=\""mj-column-per-50 outlook-group-fix\"" id=\""quotColumn\"">"" +
                            $@""<table><tbody><tr><td><div><p><span> {(quotation.ToCurrency.ToString() == nameof(USD) ? "US$" : "$")} {quotation.Valor.ToString()} </span></p></div></td></tr></tbody></table>" +
                                     $@"</div></td></tr></tbody></table></div>" +
                                     $"<div id=\"separator\"><table ><tbody><tr><td ><div class=\"mj-column-per-50 outlook-group-fix\" id=\"sepColumn\">" +
                                     $"<table><tbody><tr><td style=\"padding:0px 10px;padding-top:0px;word-break:break-word;\"><p></p>" +
                                     $"</td></tr></tbody></table></div></td></tr></tbody></table></div>";
                        }
                    }
                }
                catch (Exception ex)
                {
                    Log.Error(ex, "Error en NotifyIncomingMessage(): No se pudo procesar el body del mensaje a enviar.");
                    Monitoreo.Monitor.Critical("QuotationBotService.NotifyIncomingMessage(): No se pudo procesar el body del mensaje a enviar", _servicios.TCMail);
                }

                var htmlTable = Strings.Replace(table, '\\'.ToString(), "");

                var localTime = LocalDateTime.GetDateTimeNow();
                var dateAdded = htmlBody.Replace("{{DATE_NOW}}", $"{localTime}");
                var body = dateAdded.Replace("{{QUOTATIONS}}", htmlTable);
                var response = message.GetResponseObject(body);

                await messageChannel.SendMessageAsync(response);

                Log.Debug($"Se responde Cotización. MensajeRecibido: {message.Text()}. Respuesta: {response.Text()}. MessageChannel: {messageChannel}");
                Monitoreo.Monitor.Ok($"QuotationBot.NotifyIncomingMessage(): Se responde Cotización", _servicios.TCMail);
            }
            catch (Exception e)
            {
                Log.Error(e, $"Error al responder Cotizacion Bot. MensajeRecibido: {message.Text()}. MessageChannel: {messageChannel}");
                Monitoreo.Monitor.Critical("QuotationBotService.NotifyIncomingMessage(): Error al responder Cotizacion Bot", _servicios.TCMail);
            }
        }
    }
}
