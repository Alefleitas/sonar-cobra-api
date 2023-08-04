using System.Net.Http.Headers;
using AngleSharp.Html.Parser;
using Microsoft.Extensions.Options;
using Newtonsoft.Json;
using nordelta.cobra.service.quotations.Configuration;
using nordelta.cobra.service.quotations.Extension;
using nordelta.cobra.service.quotations.Jobs;
using nordelta.cobra.service.quotations.Services.Contracts;
using nordelta.cobra.service.quotations.Models;
using nordelta.cobra.service.quotations.Utils;
using ServiceStack;
using Newtonsoft.Json.Linq;
using CoinAPI.REST.V1;
using nordelta.cobra.service.quotations.Models.Enums;
using nordelta.cobra.service.quotations.Models.InvertirOnline;
using Monitoreo = Nordelta.Monitoreo;
using nordelta.cobra.service.quotations.Models.InvertirOnline.Endpoint;
using nordelta.cobra.service.quotations.Models.InvertirOnline.Types;
using nordelta.cobra.service.quotations.Models.InvertirOnline.Service.Contract;
using System.Data;
using nordelta.cobra.service.quotations.Models.OMS;

namespace nordelta.cobra.service.quotations.Services
{
    public class QuotationService : IQuotationService
    {
        private readonly IOptions<InvertirOnlineConfiguration> _invertirOnline;
        private readonly ITokenService _tokenService;
        private readonly IOptions<CobraApiConfiguration> _cobraApi;
        private readonly ILogger<QuotationService> _logger;
        private readonly IConfiguration _configuration;
        private readonly IHolidayService _holidayService;
        private readonly IOptions<DolarMepEspeciesConfiguration> _dolarEspeciesOption;
        private readonly IOptions<QuotationBcuConfiguration> _quotationBcuConfig;
        private readonly IOptions<QuotationBnaConfiguration> _quotationBnaConfig;
        private readonly IOptions<QuotationBcraConfiguration> _quotationBcraConfig;
        private readonly IOptions<QuotationCacConfiguration> _quotationCacConfig;
        private readonly IOptions<QuotationCoinApiConfiguration> _quotationCoinApiConfig;
        private readonly IOptions<ServiciosConfiguration> _serviciosConfig;
        private readonly IOptions<DolarMepEspeciesConfiguration> _dolarMep;


        public QuotationService(
            IOptions<InvertirOnlineConfiguration> invertirOnline,
            ITokenService tokenService,
            IOptions<CobraApiConfiguration> cobraApi,
            ILogger<QuotationService> logger,
            IConfiguration configuration,
            IHolidayService holidayService,
            IOptions<DolarMepEspeciesConfiguration> dolarEspeciesOption,
            IOptions<QuotationBcuConfiguration> quotationBcuConfig,
            IOptions<QuotationBnaConfiguration> quotationBnaConfig,
            IOptions<QuotationBcraConfiguration> quotationBcraConfig,
            IOptions<QuotationCacConfiguration> quotationCacConfig,
            IOptions<QuotationCoinApiConfiguration> quotationCoinApiConfig,
            IOptions<ServiciosConfiguration> serviciosConfig,
            IOptions<DolarMepEspeciesConfiguration> dolarMep)
        {
            _invertirOnline = invertirOnline;
            _tokenService = tokenService;
            _cobraApi = cobraApi;
            _logger = logger;
            _configuration = configuration;
            _holidayService = holidayService;
            _dolarEspeciesOption = dolarEspeciesOption;
            _quotationBcuConfig = quotationBcuConfig;
            _quotationBnaConfig = quotationBnaConfig;
            _quotationBcraConfig = quotationBcraConfig;
            _quotationCacConfig = quotationCacConfig;
            _quotationCoinApiConfig = quotationCoinApiConfig;
            _serviciosConfig = serviciosConfig;
            _dolarMep = dolarMep;
        }


        public async Task<List<Titulo>> GetQuotationsAsync()
        {
            var accessToken = await _tokenService.GetTokenAsync();
            var titulos = new List<Titulo>();
            try
            {
                _logger.LogDebug("Starting call to GetQuotationsAsync...");

                var url = _invertirOnline.Value.Url
                    .AppendPath("api", "v2")
                    .AppendUrlPathsRaw(ApiRoute.Bonos.Fmt(Uri.EscapeDataString(PanelTypes.SoberanosEnDolares)));

                var quotationRequest = await url
                    .GetJsonFromUrlAsync(req =>
                        req.With(c =>
                        {
                            c.SetAuthBearer(accessToken);
                        })
                    );
                var quotationResponse = quotationRequest.FromJson<Bonos>();

                return quotationResponse.Titulos.ToList();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error in GetQuotationsAsync");

                return titulos;
            }
        }

        //Quotations from Serie Historica MEP - Invertir Online
        public async Task<QuotationExternal> GetSerieHistoricaAsync(DateTime? dateFrom)
        {
            var accessToken = await _tokenService.GetTokenAsync();
            var dolarMep = new QuotationExternal();
            var titulos = new List<Titulo>();
            var partialQuotations = new List<double>();
            var bonos = await GetBonosConfigAsync("GetBonosConfig");

            try
            {
                var dateTo = dateFrom.Value.AddDays(1);
                _logger.LogDebug("Starting call to GetQuotationsAsync...");

                if (!await _holidayService.IsAHolidayAsync(dateFrom.Value, null))
                {
                    if (bonos.Any())
                    {
                        foreach (var bono in bonos)
                        {
                            var url = _invertirOnline.Value.Url
                            .AppendPath("api", "v2")
                            .AppendUrlPathsRaw(ApiRoute.CotizacionHistorica.Fmt(MercadoTypes.BCBA, bono.Title, dateFrom.Value.ToString("yyyy-MM-dd"), dateTo.ToString("yyyy-MM-dd"), PanelTypes.SinAjustar));

                            var url2 = _invertirOnline.Value.Url
                            .AppendPath("api", "v2")
                            .AppendUrlPathsRaw(ApiRoute.CotizacionHistorica.Fmt(MercadoTypes.BCBA, $"{bono.Title}D", dateFrom.Value.ToString("yyyy-MM-dd"), dateTo.ToString("yyyy-MM-dd"), PanelTypes.SinAjustar));

                            var reqBonoArs = await url
                                .GetJsonFromUrlAsync(req =>
                                    req.With(c =>
                                    {
                                        c.SetAuthBearer(accessToken);
                                    })
                                );

                            var reqBonoUsd = await url2
                               .GetJsonFromUrlAsync(req =>
                                   req.With(c =>
                                   {
                                       c.SetAuthBearer(accessToken);
                                   })
                               );

                            var titulosArs = reqBonoArs.FromJson<List<CotizacionDetalle>>();
                            var titulosUsd = reqBonoUsd.FromJson<List<CotizacionDetalle>>();


                            var bonoArs = titulosArs.OrderByDescending(x => x.FechaHora)
                                                            .FirstOrDefault();



                            var bonoUsd = titulosUsd.OrderByDescending(x => x.FechaHora)
                                                            .FirstOrDefault();

                            double valorUSD = bonoUsd is not null ? bonoUsd.UltimoPrecio : default;
                            double valorARS = bonoArs is not null ? bonoArs.UltimoPrecio : default;

                            if (valorUSD > 0 && valorARS > 0)
                            {
                                //mep value added
                                partialQuotations.Add(Math.Round((valorARS / valorUSD), 2));
                            }
                        }

                        double sum = partialQuotations.Sum(x => x);
                        double dolarMepValue = Math.Round((sum / partialQuotations.Count()), 2);
                        var effectiveDateFrom = dateFrom.Value.AddDays(1);
                        _logger.LogInformation($"Dolar MEP Value: {dolarMepValue}");
                        var effectiveDateTo = await _holidayService.GetNextWorkDayFromDateAsync(effectiveDateFrom);
                        _logger.LogDebug("GetNextWorkDayFromDateAsync Successfully");
                        _logger.LogInformation($"EffectiveDate: {effectiveDateFrom:dd/MM/yyyy}");

                        dolarMep = new QuotationExternal
                        {
                            EffectiveDateFrom = new DateTime(effectiveDateFrom.Year, effectiveDateFrom.Month,
                                effectiveDateFrom.Day, 0, 0, 0, 0),
                            EffectiveDateTo = new DateTime(effectiveDateTo.Year, effectiveDateTo.Month,
                                effectiveDateTo.Day, 23, 59, 59, 999),
                            Description = $"Valor MEP Dolar InvertirOnline - Especies: {string.Join(" | ", bonos.Select(x => x.Title).ToList())}  {effectiveDateFrom:dd/MM/yyyy}",
                            Especie = string.Join(" | ", bonos.Select(x => x.Title).ToList()),
                            ToCurrency = "ARS",
                            FromCurrency = "USD",
                            Source = EQuotationSource.INVERTIRONLINE,
                            Valor = dolarMepValue
                        };

                        return dolarMep;
                    }

                }

            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error in GetQuotationsAsync");

            }

            return dolarMep;
        }

        // Quotations from BCU
        public async Task<List<QuotationExternal>> GetQuotationsFromBcuAsync(DateTime? date = null)
        {
            var quotationsBcu = new List<QuotationExternal>();
            var dateNow = LocalDateTime.GetDateTimeNow();
            var dateRequest = date != null ? date.Value : dateNow;

            try
            {
                if (!await _holidayService.IsAHolidayAsync(dateRequest, null))
                {
                    var url = _quotationBcuConfig.Value.Url + "/_layouts/15/BCU.Cotizaciones/handler/CotizacionesHandler.ashx?op=getcotizaciones";

                    var quotationRequest = await url.PostJsonToUrlAsync("{\"KeyValuePairs\":" +
                                                                        "{\"Monedas\": " + JsonConvert.SerializeObject(_quotationBcuConfig.Value.Monedas) + "," +
                                                                         "\"FechaDesde\": \"" + dateRequest.ToString("dd/MM/yyyy") + "\"," +
                                                                         "\"FechaHasta\":\"" + dateRequest.ToString("dd/MM/yyyy") + "\"," +
                                                                         "\"Grupo\":\" 2 \"}}",
                                                                        requestFilter: req => { req.Headers.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json")); });

                    _logger.LogDebug($"{_quotationBcuConfig.Value.Url} response {JToken.Parse(quotationRequest)}");

                    var quotationResponse = JsonConvert.DeserializeObject<dynamic>(quotationRequest);

                    var quotations = quotationResponse != null ? (List<dynamic>)JsonConvert.DeserializeObject<List<dynamic>>(JsonConvert.SerializeObject(quotationResponse.cotizacionesoutlist.Cotizaciones)) : new List<dynamic>();

                    if (quotations != null && quotations.Any())
                    {
                        var effectiveDateFrom = dateRequest.AddDays(1);
                        var effectiveDateTo = await _holidayService.GetNextWorkDayFromDateAsync(effectiveDateFrom);
                        _logger.LogDebug("GetNextWorkDayFromDateAsync Successfully");

                        foreach (var quotation in quotations) // USD -> UYU / ARS -> UYU
                        {
                            if (quotation.TCC == 0 || quotation.TCV == 0)
                            {
                                _logger.LogWarning($"Warning: Currency : {quotation.CodigoISO ?? "Invalid"} - Compra: {quotation.TCC} - Venta: {quotation.TCV} of the date: {dateNow:dd/MM/yyyy} in GetQuotationsFromBcuAsync");
                                continue;
                            }

                            foreach (var rateType in new List<string> { "Compra", "Venta" }) // Compra / Venta
                            {
                                var newQuotation = new QuotationExternal
                                {
                                    EffectiveDateFrom = new DateTime(effectiveDateFrom.Year, effectiveDateFrom.Month,
                                        effectiveDateFrom.Day, 0, 0, 0, 0),
                                    EffectiveDateTo = new DateTime(effectiveDateTo.Year, effectiveDateTo.Month,
                                        effectiveDateTo.Day, 23, 59, 59, 999),
                                    Description = $"Valor {(quotation.CodigoISO == "DLS." ? "USD" : quotation.CodigoISO)} {rateType} de BCU - {effectiveDateFrom:dd/MM/yyyy}",
                                    FromCurrency = quotation.CodigoISO == "DLS." ? "USD" : quotation.CodigoISO,
                                    ToCurrency = "UYU",
                                    RateType = rateType == "Compra" ? RateTypes.BillBcuComprador : rateType == "Venta" ? RateTypes.BillBcuVendedor : String.Empty,
                                    Source = EQuotationSource.BCU,
                                    Valor = rateType == "Compra" ? quotation.TCC : rateType == "Venta" ? quotation.TCV : 0
                                };
                                _logger.LogInformation($"new quotation from BCU: {JToken.Parse(JsonConvert.SerializeObject(newQuotation))}");
                                quotationsBcu.Add(newQuotation);
                            }
                        }
                    }
                    else
                    {
                        _logger.LogWarning($"Warning: BCU does not have quotations for the date: {dateNow:dd/MM/yyyy}");
                    }
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error in GetQuotationsFromBcuAsync");
            }

            return quotationsBcu;
        }

        // Quotation from BNA
        public async Task<List<QuotationExternal>> GetQuotationsFromBnaAsync(DateTime? date = null)
        {
            var quotationsBna = new List<QuotationExternal>();
            var dateNow = date != null ? date : LocalDateTime.GetDateTimeNow();
            var dateRequest = date != null ? date.Value.ToString("d/M/yyyy") : dateNow.Value.ToString("d/M/yyyy");

            try
            {
                if (!await _holidayService.IsAHolidayAsync(dateNow.Value, null))
                {
                    var effectiveDateFrom = dateNow.Value.AddDays(1);
                    var effectiveDateTo = await _holidayService.GetNextWorkDayFromDateAsync(effectiveDateFrom);
                    _logger.LogDebug("GetNextWorkDayFromDateAsync Successfully");

                    foreach (var type in _quotationBnaConfig.Value.Types) // billetes / monedas
                    {
                        string Url = $"{_quotationBnaConfig.Value.Url}/Cotizador/HistoricoPrincipales?id={type.Id}&fecha={dateRequest}&filtroEuro=1&filtroDolar=1";
                        var htmlResponse = await Url.GetJsonFromUrlAsync();
                        var document = await new HtmlParser().ParseDocumentAsync(htmlResponse);

                        foreach (var table in type.Tables) // tablaDolar / tablaEuro
                        {
                            var row = document.GetElementById(table)?.QuerySelector("table")?.QuerySelector("tbody")?.QuerySelectorAll("tr").FirstOrDefault(x => x.QuerySelectorAll("td")[3].TextContent == dateRequest)?.QuerySelectorAll("td");

                            var Moneda = row?[0].TextContent == "Dolar U.S.A" ? "USD" : row?[0].TextContent == "Euro" ? "EUR" : string.Empty;
                            var TCC = (row?[1].TextContent ?? "0").Trim().GetDouble();
                            var TCV = (row?[2].TextContent ?? "0").Trim().GetDouble();

                            if ((Moneda != "USD" && Moneda != "EUR") || TCC == 0 || TCV == 0)
                            {
                                _logger.LogWarning($"Warning: Moneda : {(String.IsNullOrEmpty(Moneda) ? "Invalid" : Moneda)} - Compra: {TCC} - Venta: {TCV} of the table: {table} and of the type: {type.Id} and of the date: {dateNow:dd/MM/yyyy} in GetQuotationsFromBnaAsync");
                                continue;
                            }

                            foreach (var rateType in new List<string> { "Compra", "Venta" }) // Compra / Venta
                            {
                                var newQuotation = new QuotationExternal()
                                {
                                    EffectiveDateFrom = new DateTime(effectiveDateFrom.Year, effectiveDateFrom.Month,
                                                effectiveDateFrom.Day, 0, 0, 0, 0),
                                    EffectiveDateTo = new DateTime(effectiveDateTo.Year, effectiveDateTo.Month,
                                                effectiveDateTo.Day, 23, 59, 59, 999),

                                    Description = $"Valor {Moneda}_ARS {rateType} - {effectiveDateFrom:dd/MM/yyyy}",
                                    FromCurrency = Moneda,
                                    ToCurrency = "ARS",
                                    RateType = rateType == "Compra" && type.Id == "billetes" ? RateTypes.BillBnaComprador : rateType == "Compra" && type.Id == "monedas" ? RateTypes.DivBnaComprador :
                                               rateType == "Venta" && type.Id == "billetes" ? RateTypes.BillBnaVendedor : rateType == "Venta" && type.Id == "monedas" ? RateTypes.DivBnaVendedor : String.Empty,
                                    Source = EQuotationSource.BNA,
                                    Valor = Math.Round(rateType == "Compra" ? TCC : rateType == "Venta" ? TCV : 0, 2)
                                };
                                _logger.LogInformation($"new quotation from BNA: {JToken.Parse(JsonConvert.SerializeObject(newQuotation))}");
                                quotationsBna.Add(newQuotation);
                            }
                        }
                    }
                }
                else
                {
                    _logger.LogWarning($"Warning: BNA does not have quotations for the date: {dateNow:dd/MM/yyyy}");
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error in GetQuotationsFromBnaAsync");
            }
            return quotationsBna;
        }

        // Quotation from BCRA
        public async Task<QuotationExternal> GetUsdMayoristaFromBcraAsync()
        {
            var dateNow = LocalDateTime.GetDateTimeNow();
            var newQuotation = new QuotationExternal();

            try
            {
                if (!await _holidayService.IsAHolidayAsync(dateNow, null))
                {
                    var Url = _quotationBcraConfig.Value.Url;

                    var htmlResponse = await Url.GetJsonFromUrlAsync();
                    var document = await new HtmlParser().ParseDocumentAsync(htmlResponse);

                    var row = document.GetElementsByClassName("table-BCRA").FirstOrDefault()?.QuerySelector("tbody")?.QuerySelectorAll("tr")?.Where(t => t.TextContent.Contains("A 3500")).FirstOrDefault()?.QuerySelectorAll("td");
                    var value = (row?[2].TextContent ?? "0").Trim().GetDouble();

                    if (value > 0)
                    {
                        var effectiveDateFrom = dateNow.AddDays(1);
                        var effectiveDateTo = await _holidayService.GetNextWorkDayFromDateAsync(effectiveDateFrom);
                        _logger.LogDebug("GetNextWorkDayFromDateAsync Successfully");

                        newQuotation = new QuotationExternal
                        {
                            EffectiveDateFrom = new DateTime(effectiveDateFrom.Year, effectiveDateFrom.Month,
                                effectiveDateFrom.Day, 0, 0, 0, 0),
                            EffectiveDateTo = new DateTime(effectiveDateTo.Year, effectiveDateTo.Month,
                                effectiveDateTo.Day, 23, 59, 59, 999),
                            Description = $"Valor BCRA Mayorista Com. A3500 - {effectiveDateFrom:dd/MM/yyyy}",
                            FromCurrency = "USD",
                            ToCurrency = "ARS",
                            RateType = RateTypes.BcraMayor,
                            Source = EQuotationSource.BCRA,
                            Valor = value
                        };
                        _logger.LogInformation($"new quotation from BCRA {JToken.Parse(JsonConvert.SerializeObject(newQuotation))}");
                    }
                }
                else
                {
                    _logger.LogWarning($"Warning: BCRA has no quotation for the date: {dateNow:dd/MM/yyyy}");
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error in GetUsdMayoristaFromBcraAsync");
            }

            return newQuotation;
        }

        // Quotation CAC from CAMARCO
        public async Task<QuotationExternal> GetCacAsync()
        {
            var currentDate = LocalDateTime.GetDateTimeNow();
            try
            {
                var url = _quotationCacConfig.Value.Url;

                var htmlResponse = await url.GetStringFromUrlAsync();
                var document = await new HtmlParser().ParseDocumentAsync(htmlResponse);

                var webDate = document.GetElementsByClassName("vc_custom_heading")[2].TextContent;
                var cotizacionString = document.GetElementsByClassName("vc_custom_heading")[3].TextContent;
                _logger.LogInformation($"{webDate} Cotizacion CAC: {cotizacionString ?? "valor nulo"}");
                var cotizacion = (string.IsNullOrEmpty(cotizacionString) ? "0" : cotizacionString).Trim().GetDouble();

                if (cotizacion > 0)
                {
                    var dateFromEffective = new DateTime(currentDate.Year, currentDate.Month, currentDate.Day, 0, 0, 0, 000);
                    var dateToEffective = new DateTime(
                        dateFromEffective.AddMonths(1).Year, //year of the date
                        dateFromEffective.AddMonths(1).Month, //month of the date
                        DateTime.DaysInMonth(dateFromEffective.AddMonths(1).Year, dateFromEffective.AddMonths(1).Month), //last day of the resultant month
                        23, 59, 59, 999); //last second of the day

                    var newQuotation = new QuotationExternal
                    {
                        EffectiveDateFrom = dateFromEffective,
                        EffectiveDateTo = dateToEffective,
                        Description = $"Valor CAC - {webDate}",
                        RateType = RateTypes.Cac,
                        FromCurrency = "CAC",
                        ToCurrency = "ARS",
                        Source = EQuotationSource.CAMARCO,
                        Valor = cotizacion
                    };
                    _logger.LogInformation($"new quotation from CAMARCO: {JToken.Parse(JsonConvert.SerializeObject(newQuotation))}");

                    return newQuotation;
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error in GetCacAsync");
            }
            return new QuotationExternal();
        }

        // Quotations from CoinApi
        public async Task<List<QuotationExternal>> GetQuotationsFromCoinApiAsync(DateTime? date = null)
        {
            var quotations = new List<QuotationExternal>();
            var dateNow = LocalDateTime.GetDateTimeNow();
            var dateRequest = date != null ? date.Value : dateNow;

            try
            {
                var apiKey = _quotationCoinApiConfig.Value.ApiKey;

                if (!await _holidayService.IsAHolidayAsync(dateRequest, null))
                {
                    var effectiveDateFrom = dateRequest.AddDays(1);
                    var effectiveDateTo = await _holidayService.GetNextWorkDayFromDateAsync(effectiveDateFrom);
                    _logger.LogDebug("GetNextWorkDayFromDateAsync Successfully");

                    var coinApiEndpointTester = new CoinApiRestEndpointsTester(apiKey)
                    {
                        Log = s => Console.WriteLine(s)
                    };

                    foreach (var exchangeRate in _quotationCoinApiConfig.Value.ExchangeRates)
                    {
                        double value = 0;

                        if (date != null)
                        {
                            value = Math.Round(Decimal.ToDouble((await coinApiEndpointTester.Exchange_rates_get_specific_rateAsync(exchangeRate.BaseId, exchangeRate.QuoteId, date.Value)).Data.rate), 2);

                        }
                        else
                        {
                            value = Math.Round(Decimal.ToDouble((await coinApiEndpointTester.Exchange_rates_get_specific_rateAsync(exchangeRate.BaseId, exchangeRate.QuoteId)).Data.rate), 2);

                        }

                        if (value > 0)
                        {
                            var newQuotation = new QuotationExternal
                            {
                                EffectiveDateFrom = new DateTime(effectiveDateFrom.Year, effectiveDateFrom.Month,
                                effectiveDateFrom.Day, 0, 0, 0, 0),
                                EffectiveDateTo = new DateTime(effectiveDateTo.Year, effectiveDateTo.Month,
                                effectiveDateTo.Day, 23, 59, 59, 999),
                                Description = $"Valor {exchangeRate.BaseId} - {effectiveDateFrom:dd/MM/yyyy}",
                                FromCurrency = exchangeRate.BaseId,
                                ToCurrency = exchangeRate.QuoteId,
                                RateType = RateTypes.Cryptos,
                                Source = EQuotationSource.COINAPI,
                                Valor = value
                            };
                            _logger.LogInformation($"new quotation from CoinApi: {JToken.Parse(JsonConvert.SerializeObject(newQuotation))}");
                            quotations.Add(newQuotation);
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error in GetQuotationsFromCoinApiAsync");
            }
            return quotations;
        }

        public async Task<bool> InformQuotationAsync(QuotationExternal quotation, string endpoint)
        {
            var succeeded = false;

            try
            {
                var url = _cobraApi.Value.Url
                    .AppendPath("Quotation", endpoint);

                var informQuoteRequest = await url
                    .PostJsonToUrlAsync(quotation,
                        req =>
                        {
                            req.Headers.Authorization = AuthenticationHeaderValue.Parse(_cobraApi.Value.Token);
                        });

                var informQuoteRequestResponse = JsonConvert.DeserializeObject<dynamic>(informQuoteRequest);
                _logger.LogDebug($"InformQuotationAsync response: {JToken.Parse(JsonConvert.SerializeObject(informQuoteRequestResponse))}");

                if (informQuoteRequestResponse?.GetType().Name == "String")
                    succeeded = false;
                else
                    succeeded = informQuoteRequestResponse?.id > 0;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error when inform quotation to Quotation/{endpoint}");
            }

            return succeeded;
        }

        public async Task<bool> InformQuotationsAsync(List<QuotationExternal> quotations, string endpoint)
        {
            var succeeded = false;

            try
            {
                var url = _cobraApi.Value.Url
                    .AppendPath("Quotation", endpoint);

                var informQuoteRequest = await url
                    .PostJsonToUrlAsync(quotations,
                        req =>
                        {
                            req.Headers.Authorization = AuthenticationHeaderValue.Parse(_cobraApi.Value.Token);
                        });

                var informQuoteRequestResponse = JsonConvert.DeserializeObject<List<dynamic>>(informQuoteRequest);

                _logger.LogDebug($"InformQuotationsAsync response: {JToken.Parse(JsonConvert.SerializeObject(informQuoteRequestResponse))}");
                succeeded = informQuoteRequestResponse?.Count > 0;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error when inform quotations to Quotation/{endpoint}");
            }

            return succeeded;
        }
        public async Task<bool> InformMepQuotationAsync(DolarMEP quotation)
        {
            var succeeded = false;

            try
            {
                _logger.LogDebug("Starting call to InformMepQuotationAsync...");
                var url = _cobraApi.Value.Url
                    .AppendPath("Quotation", "AddMepQuotationExternal")
                    .AddQueryParam("quotationType", "DolarMEP");

                var informQuoteRequest = await url
                        .PostJsonToUrlAsync(quotation,
                            req =>
                            {
                                req.Headers.Authorization =
                                    AuthenticationHeaderValue.Parse(_cobraApi.Value.Token);
                            });

                var informQuoteRequestResponse = informQuoteRequest.FromJson<int>();
                _logger.LogDebug($"InformMepQuotationAsync response: {informQuoteRequestResponse}");

                succeeded = informQuoteRequestResponse > 0;

                return succeeded;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error in InformMepQuotation");

                return succeeded;
            }
        }


        public async Task GetDolarMepValueAsync()
        {
            var partialQuotations = new List<double>();
            var dateNow = LocalDateTime.GetDateTimeNow();
            var bonosConfig = await GetBonosConfigAsync("GetBonosConfig");

            try
            {
                if (!await _holidayService.IsAHolidayAsync(dateNow, null))
                {
                    var loginToken = await _tokenService.GetTokenAsync();
                    if (!string.IsNullOrEmpty(loginToken))
                    {
                        var cotizaciones = await GetQuotationsAsync();
                        if (cotizaciones.Any())
                        {
                            if (bonosConfig.Any())
                            {
                                foreach (var bono in bonosConfig)
                                {
                                    var valorBonoUSD = cotizaciones.FirstOrDefault(x => x.Simbolo.Equals($"{bono.Title}D"));
                                    double valorUSD = valorBonoUSD is not null ? valorBonoUSD.UltimoPrecio.GetDouble() : default;
                                    var valorBonoARS = cotizaciones.FirstOrDefault(x => x.Simbolo.Equals($"{bono.Title}"));
                                    double valorARS = valorBonoARS is not null ? valorBonoARS.UltimoPrecio.GetDouble() : default;

                                    if (valorUSD > 0 && valorARS > 0)
                                    {
                                        //mep value added
                                        partialQuotations.Add(Math.Round((valorARS / valorUSD), 2));
                                    }
                                }
                                //get average
                                double sum = partialQuotations.Sum(x => x);
                                double dolarMepValue = Math.Round((sum / partialQuotations.Count()), 2);
                                var effectiveDateFrom = dateNow.AddDays(1);
                                _logger.LogInformation($"Dolar MEP Value: {dolarMepValue}");
                                var effectiveDateTo = await _holidayService.GetNextWorkDayFromDateAsync(effectiveDateFrom);
                                _logger.LogDebug("GetNextWorkDayFromDateAsync Successfully");
                                _logger.LogInformation($"EffectiveDate: {effectiveDateFrom:dd/MM/yyyy}");

                                var dolarMep = new DolarMEP
                                {
                                    EffectiveDateFrom = new DateTime(effectiveDateFrom.Year, effectiveDateFrom.Month,
                                        effectiveDateFrom.Day, 0, 0, 0, 0),
                                    EffectiveDateTo = new DateTime(effectiveDateTo.Year, effectiveDateTo.Month,
                                        effectiveDateTo.Day, 23, 59, 59, 999),
                                    Description = $"Valor MEP Dolar InvertirOnline - Especies: {string.Join(" | ", bonosConfig.Select(x => x.Title).ToList())}  {effectiveDateFrom:dd/MM/yyyy}",
                                    Especie = string.Join(" | ", bonosConfig.Select(x => x.Title).ToList()),
                                    Source = EQuotationSource.INVERTIRONLINE,
                                    Valor = dolarMepValue
                                };

                                var result = await InformMepQuotationAsync(dolarMep);

                                _logger.LogDebug(result ? "InformMepQuotationAsync Successfully" : "InformMepQuotationAsync Failed");

                                if (result)
                                    Monitoreo.Monitor.Ok($"Se informaron las cotizaciones de USD MEP obtenidas", _serviciosConfig.Value.UsdMep);
                                else
                                    Monitoreo.Monitor.Critical($"Ocurrió un error al intentar informar la cotización", _serviciosConfig.Value.UsdMep);
                            }
                        }
                    }
                }

            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error in {typeof(DolarMepJob)}");
                Monitoreo.Monitor.Critical($"Ocurrió un error al obtener las cotizaciones de USD MEP", _serviciosConfig.Value.UsdMep);
            }

        }

        public async Task<List<BonoConfig>> GetBonosConfigAsync(string endpoint)
        {
            var bonosConfig = new List<BonoConfig>();
            try
            {
                var url = _cobraApi.Value.Url
                   .AppendPath("Quotation", endpoint);

                var request = await url
                   .GetJsonFromUrlAsync(
                       req =>
                       {
                           req.Headers.Authorization = AuthenticationHeaderValue.Parse(_cobraApi.Value.Token);
                       });

                bonosConfig = JsonConvert.DeserializeObject<List<BonoConfig>>(request);

                _logger.LogDebug($"Request response: {JToken.Parse(JsonConvert.SerializeObject(bonosConfig))}");

                if (bonosConfig == null || !bonosConfig.Any())
                {
                    bonosConfig.Add(new BonoConfig
                    {
                        Title = $"{_dolarEspeciesOption.Value.BonoARS}"
                    });
                }

                return bonosConfig;
            }
            catch (Exception ex)
            {
                _logger.LogError($"QuotationService > GetBonoConfigAsync > Error request to Endpoint/{endpoint}: ", ex);
            }

            return bonosConfig;

        }

        public async Task<List<QuotationExternal>> GetSourceQuotationsAsync(DateTime date, IEnumerable<string> quotes)
        {
            var filteredQuotes = new List<QuotationExternal>();

            try
            {
                foreach (var quote in quotes)
                {
                    var quotations = await GetSourceTypeAsync(date, quote);
                    filteredQuotes.AddRange(quotations);
                }

                return filteredQuotes;

            }
            catch (Exception ex)
            {
                _logger.LogError("SaveQuotesByDateAsync > InformQuotationsAsync > Error", ex);
            }

            return filteredQuotes;
        }

        private async Task<List<QuotationExternal>> GetSourceTypeAsync(DateTime date, string quoteType)
        {
            var result = new List<QuotationExternal>();

            var options = new Dictionary<string, Func<DateTime, Task<object>>>()
            {
                {EQuotationSource.COINAPI.ToString(), async d => await GetQuotationsFromCoinApiAsync(d)},
                {EQuotationSource.BCRA.ToString(), async d => await GetUsdMayoristaFromBcraAsync()},
                {EQuotationSource.BNA.ToString(), async d => await GetQuotationsFromBnaAsync(d)},
                {EQuotationSource.BCU.ToString(), async d => await GetQuotationsFromBcuAsync(d)},
                {EQuotationSource.INVERTIRONLINE.ToString(), async d => await GetSerieHistoricaAsync(d)}
            };

            if (options.TryGetValue(quoteType, out var getQuotations))
            {
                var quotations = await getQuotations(date);

                if (quotations is List<QuotationExternal> cotizacionesLista)
                {
                    result.AddRange(cotizacionesLista);
                }
                else if (quotations is QuotationExternal cotizacionIndividual)
                {
                    result.Add(cotizacionIndividual);
                }
            }

            return result;
        }

        public async Task<List<BonoConfig>> GetEspeciesAsync()
        {
            var dateNow = LocalDateTime.GetDateTimeNow();
            var bonos = new List<BonoConfig>();
            try
            {
                if (!await _holidayService.IsAHolidayAsync(dateNow, null))
                {
                    var loginToken = await _tokenService.GetTokenAsync();

                    if (!string.IsNullOrEmpty(loginToken))
                    {
                        var cotizaciones = await GetQuotationsAsync();

                        if (cotizaciones.Any())
                        {
                            foreach (var ctz in cotizaciones)
                            {
                                var valorBonoUSD = cotizaciones.FirstOrDefault(x => x.Simbolo.Equals($"{ctz.Simbolo}D"));
                                double valorUSD = valorBonoUSD is not null ? valorBonoUSD.UltimoPrecio.GetDouble() : default;
                                var valorBonoARS = cotizaciones.FirstOrDefault(x => x.Simbolo.Equals($"{ctz.Simbolo}"));
                                double valorARS = valorBonoARS is not null ? valorBonoARS.UltimoPrecio.GetDouble() : default;

                                if (valorUSD > 0 && valorARS > 0)
                                {

                                    bonos.Add(new BonoConfig
                                    {
                                        Title = $"{ctz.Simbolo} | {ctz.Simbolo}D",
                                        Value = Math.Round((valorARS / valorUSD), 2)
                                    });

                                }

                            }

                        }
                    }
                }
            }
            catch (Exception ex)
            {
                _logger.LogError("GetEspeciesAsync > Error:", ex);
                Monitoreo.Monitor.Critical($"Ocurrió un error al obtener las especies de Invertir Online", _serviciosConfig.Value.UsdMep);

            }

            return bonos;
        }


    }

}
