using Microsoft.Extensions.Options;
using nordelta.cobra.service.quotations.Configuration;
using nordelta.cobra.service.quotations.Models.InvertirOnline;
using nordelta.cobra.service.quotations.Services.Contracts;
using nordelta.cobra.service.quotations.Utils;
using System.Net.Http.Headers;
using System.Text;
using ServiceStack;
using nordelta.cobra.service.quotations.Models.InvertirOnline.Endpoint;
using nordelta.cobra.service.quotations.Models.InvertirOnline.Types;
using nordelta.cobra.service.quotations.Models;
using nordelta.cobra.service.quotations.Models.OMS;
using nordelta.cobra.service.quotations.Models.OMS.Routes;
using System.Text.Json;
using Newtonsoft.Json.Linq;
using System.Globalization;
using Newtonsoft.Json;
using System.Net.Http.Json;
using AngleSharp.Html.Parser;
using nordelta.cobra.service.quotations.Models.DolarHoy;
using nordelta.cobra.service.quotations.Models.Enums;
using RestSharp;
using nordelta.cobra.service.quotations.Models.OMS.Types;

namespace nordelta.cobra.service.quotations.Services
{
    public class FinanceQuotationsService : IQuoteService
    {

        private readonly IOptions<OMSConfiguration> _omsAPI;
        private readonly IOptions<InvertirOnlineConfiguration> _invertirOnline;
        private readonly IOptions<DolarHoyConfiguration> _webDolarHoy;
        private readonly IOptions<CobraApiConfiguration> _cobraApi;
        private readonly ILogger<QuotationService> _logger;
        private readonly IConfiguration _configuration;
        private readonly IHttpClientFactory _httpClientFactory;

        readonly JsonSerializerOptions _options = new JsonSerializerOptions
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase
        };
        public string OmsToken { get; set; }

        public FinanceQuotationsService(
            IOptions<OMSConfiguration> omsAPI,
            IOptions<InvertirOnlineConfiguration> invertirOnline,
            IOptions<DolarHoyConfiguration> webDolarHoy,
            IOptions<CobraApiConfiguration> cobraApi,
            ILogger<QuotationService> logger,
            IConfiguration configuration,
            IHttpClientFactory httpClientFactory
            )
        {
            _omsAPI = omsAPI;
            _invertirOnline = invertirOnline;
            _webDolarHoy = webDolarHoy;
            _cobraApi = cobraApi;
            _logger = logger;
            _configuration = configuration;
            _httpClientFactory = httpClientFactory;
            OmsToken = "";
        }

        #region GetQuotationsMethods

        public async Task<Bono> GetBonoAsync(Dictionary<string, string> query)
        {
            var bono = new Bono();
            var url = _omsAPI.Value.ServerUrl;
            var endpoint = RequestUriUtil.GetUriWithQueryString(ApiUrl.MarketData, query);

            try
            {
                await CheckTokenOmsAsync();

                var client = new RestClient(_omsAPI.Value.ServerUrl);
                var request = new RestRequest(endpoint, Method.Get);
                request.AddHeader("Content-Type", "application/json");
                request.AddHeader("Authorization", $"Bearer {OmsToken}");
                var body = @"";
                request.AddParameter("application/json", body, ParameterType.RequestBody);
                var response = client.Execute(request);

                if (response.IsSuccessStatusCode)
                {
                    JsonConvert.PopulateObject(response.Content, bono);
                }

                return bono;

            }
            catch (Exception ex)
            {
                _logger.LogError($"Error in GetBonoAsync: {endpoint}", ex.Message);
            }

            return bono;
        }
        public async Task<Indice> GetIndiceMervalAsync()
        {
            var indice = new Indice();
            try
            {
                var url = $"{_omsAPI.Value.ServerUrl}{ApiUrl.MarketIndex}";
                await CheckTokenOmsAsync();

                var client = _httpClientFactory.CreateClient("OMSClient");
                client.BaseAddress = new Uri(url);
                client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", OmsToken);
                client.DefaultRequestHeaders.CacheControl = new CacheControlHeaderValue() { NoCache = true };

                var body = JsonContent.Create(new { name = "M" });

                var response = await client.PostAsync(url, body);

                if (response.IsSuccessStatusCode)
                {
                    var content = await response.Content.ReadAsStringAsync();
                    indice = System.Text.Json.JsonSerializer.Deserialize<Indice>(content, _options);
                }

                return indice;

            }
            catch (Exception ex)
            {
                _logger.LogError("Error in GetIndiceMervalAsync", ex.Message);
            }

            return indice;
        }

        public async Task<List<CaucionTitulo>> GetCaucionesAsync()
        {
            _logger.LogDebug("Starting call to GetCaucionesAsync...");
            var titulos = new List<CaucionTitulo>();

            try
            {
                var accessToken = await InvertirOnlineLoginAsync();

                var url = _invertirOnline.Value.Url
                    .AppendPath("api", "v2")
                    .AppendUrlPathsRaw(ApiRoute.Cauciones.Fmt(Uri.EscapeDataString(PanelTypes.Todas)));

                var quotationRequest = await url
                    .GetJsonFromUrlAsync(req =>
                        req.With(c =>
                        {
                            c.SetAuthBearer(accessToken);
                        })
                    );
                var quotationResponse = quotationRequest.FromJson<Caucion>();

                CaucionTitulo? plazoMasCercano = null;

                for (var i = 1; i < 7; i++)
                {
                    plazoMasCercano = quotationResponse.Titulos.Where(x => x.Plazo == i).MaxBy(y => y.TasaPromedio);
                    if (plazoMasCercano != null) break;
                }

                if (plazoMasCercano == null)
                {
                    plazoMasCercano = new CaucionTitulo()
                    {
                        Plazo = 1
                    };
                    plazoMasCercano.Tooltip = new TooltipMessage()
                    {
                        Tipo = "alert",
                        Mensaje = "Error al obtener Caución (no hay plazos menores a 7 días)"
                    };
                }
                var plazo7 = quotationResponse.Titulos.Where(x => x.Plazo == 7).MaxBy(y => y.TasaPromedio);
                if (plazo7 == null)
                {
                    plazo7 = new CaucionTitulo();
                    plazo7.Plazo = 7;
                    plazo7.Tooltip = new TooltipMessage()
                    {
                        Tipo = "alert",
                        Mensaje = "Error al obtener Caución (no hay plazo 7 días)"
                    };
                }

                titulos.Add(plazoMasCercano);
                titulos.Add(plazo7);

                return titulos;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error in GetCaucionesAsync");

                var plazo1 = new CaucionTitulo();
                plazo1.Plazo = 1;
                plazo1.Tooltip = new TooltipMessage()
                {
                    Tipo = "error",
                    Mensaje = "Error al obtener Caución"
                };
                var plazo7 = new CaucionTitulo();
                plazo7.Plazo = 7;
                plazo7.Tooltip = new TooltipMessage()
                {
                    Tipo = "error",
                    Mensaje = "Error al obtener Caución"
                };

                titulos.Add(plazo1);
                titulos.Add(plazo7);
            }

            return titulos;
        }

        public async Task<List<CotizacionDetalle>> GetCotizacionDetalleAsync()
        {
            _logger.LogDebug("Starting call to GetCotizacionDetalleAsync...");
            var cotizaciones = new List<CotizacionDetalle>();

            try
            {
                var accessToken = await InvertirOnlineLoginAsync();

                var simbolos = new List<string>() { ETFAccionType.SPY, ETFAccionType.QQQ, ETFAccionType.DIA, ETFAccionType.EWZ };

                foreach(var simbolo in simbolos)
                {
                    var url = _invertirOnline.Value.Url
                        .AppendPath("api", "v2")
                        .AppendUrlPathsRaw(ApiRoute.CotizacionDetalle
                        .Fmt(Uri.EscapeDataString(MercadoTypes.NASDAQ), Uri.EscapeDataString(simbolo)));

                    var res = await url
                        .GetJsonFromUrlAsync(req =>
                            req.With(c =>
                            {
                                c.SetAuthBearer(accessToken);
                            })
                        );

                    cotizaciones.Add(res.FromJson<CotizacionDetalle>());
                }

                return cotizaciones;
            }
            catch (Exception ex)
            {
                _logger.LogError("Error in GetCotizacionDetalleAsync", ex.Message);
            }
            
            return cotizaciones;
        }

        public async Task<List<CotizacionDetalle>> GetCedearsAsync()
        {
            _logger.LogDebug("Starting call to GetCedearsAsync...");
            var cotizaciones = new List<CotizacionDetalle>();

            try
            {
                var accessToken = await InvertirOnlineLoginAsync();

                var url = _invertirOnline.Value.Url
                    .AppendPath("api", "v2")
                    .AppendUrlPathsRaw(ApiRoute.Cedears
                    .Fmt(Uri.EscapeDataString(PanelTypes.Todos)));

                var res = await url
                    .GetJsonFromUrlAsync(req =>
                        req.With(c =>
                        {
                            c.SetAuthBearer(accessToken);
                        })
                    );

                var cedears = res.FromJson<Cotizaciones>();

                var cedearsValores = cedears.Titulos.Where(x => 
                                        x.Simbolo == ETFAccionType.SPY
                                     || x.Simbolo == ETFAccionType.AAPL
                                     || x.Simbolo == ETFAccionType.KO
                                     || x.Simbolo == ETFAccionType.MELI
                                     || x.Simbolo == ETFAccionType.AMD
                                     || x.Simbolo == ETFAccionType.TSLA
                                     || x.Simbolo == ETFAccionType.AMZN
                                     || x.Simbolo == ETFAccionType.GOOGL
                                     || x.Simbolo == ETFAccionType.QQQ
                                     ).ToList();

                return cedearsValores;
            }
            catch (Exception ex)
            {
                _logger.LogError("Error in GetCedearsAsync", ex.Message);
            }

            return cotizaciones;
        }

        public async Task<List<CotizacionDetalle>> GetAccionesUsaAsync()
        {
            _logger.LogDebug("Starting call to GetAccionesUsaAsync...");
            var cotizaciones = new List<CotizacionDetalle>();

            try
            {
                var accessToken = await InvertirOnlineLoginAsync();

                var url = _invertirOnline.Value.Url
                    .AppendPath("api", "v2")
                    .AppendUrlPathsRaw(ApiRoute.AccionesUSA
                    .Fmt(Uri.EscapeDataString(PanelTypes.Todos)));

                var res = await url
                    .GetJsonFromUrlAsync(req =>
                        req.With(c =>
                        {
                            c.SetAuthBearer(accessToken);
                        })
                    );

                var acciones = res.FromJson<Cotizaciones>();

                var accionesUSA = acciones.Titulos.Where(x => 
                                        x.Simbolo == ETFAccionType.AAPL
                                     || x.Simbolo == ETFAccionType.KO
                                     || x.Simbolo == ETFAccionType.MELI
                                     || x.Simbolo == ETFAccionType.AMD
                                     || x.Simbolo == ETFAccionType.TSLA
                                     || x.Simbolo == ETFAccionType.AMZN
                                     || x.Simbolo == ETFAccionType.GOOGL
                                     ).ToList();

                return accionesUSA;
            }
            catch (Exception ex)
            {
                _logger.LogError("Error in GetAccionesUsaAsync", ex.Message);
            }

            return cotizaciones;
        }

        public async Task<DolarBlue> GetDolarBlueAsync()
        {
            DolarBlue precioDolar = new DolarBlue();
            try
            {
                var Url = _webDolarHoy.Value.Url;

                var htmlResponse = await Url.GetJsonFromUrlAsync();
                var document = await new HtmlParser().ParseDocumentAsync(htmlResponse);

                var scrapedData = document.QuerySelectorAll(".cotizacion_moneda .cotizacion_value .tile.is-child").Where(x => x.QuerySelector(".topic")?.InnerHtml.ToLower() == "venta").FirstOrDefault().QuerySelector(".value")?.InnerHtml;
                var valor = double.Parse(scrapedData.Substring(1), CultureInfo.InvariantCulture);

                if (valor > 0)
                {
                    precioDolar.Precio = valor;
                }
                else
                {
                    precioDolar.Precio = 0;
                    precioDolar.MensajeError = new TooltipMessage()
                    {
                        Tipo = "alert",
                        Mensaje = "No se pudo obtener la cotización de Dólar Blue"
                    }; ;
                }
                            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error in GetDolarBlueAsync");
                precioDolar.Precio = 0;
                precioDolar.MensajeError = new TooltipMessage()
                {
                    Tipo = "error",
                    Mensaje = "Error en la página web"
                }; ;
            }

            return precioDolar;
        }
        #endregion

        #region JobMethods
        public async Task<List<Quotes>> GetQuotesAsync()
        {

            _logger.LogDebug("Starting call to GetQuotesAsync...");
            var quotes = new List<Quotes>();

            try
            {
                //
                // Obtener cotizaciones
                //
                var detallesCotizaciones = await GetCotizacionDetalleAsync();

                var accionesUsa = await GetAccionesUsaAsync();
                accionesUsa.AddRange(detallesCotizaciones.Where(x => x.Simbolo == ETFAccionType.SPY || x.Simbolo == ETFAccionType.QQQ));
                var cedears = await GetCedearsAsync();
                var dolarCCLCedear = GetDolarCCLCedear(accionesUsa, cedears);

                var cauciones = await GetCaucionesAsync();

                var bonoGD30 = await GetBonoAsync(BonoDictionary.GD30);
                var bonoGD30D = await GetBonoAsync(BonoDictionary.GD30D);
                var bonoGD30C = await GetBonoAsync(BonoDictionary.GD30C);
                var bono48Horas = await GetBonoAsync(BonoDictionary.GD30_48Hs);
                var bono48MEP = await GetBonoAsync(BonoDictionary.GD30D_48Hs);
                var bono48CCL = await GetBonoAsync(BonoDictionary.GD30C_48Hs);

                //
                // Obtener índices
                //
                var merval = await GetIndiceMervalAsync();

                //
                // Obtener Dolar Blue
                //
                var dolarBlue = await GetDolarBlueAsync();

                //
                // Agregar datos obtenidos a lista de cotizaciones
                //

                // Dólar Blue
                quotes.Add(new Quotes
                {
                    Tipo = ETipoQuote.DOLAR,
                    Titulo = TitulosQuote.DolarBlue,
                    Descripcion = DescripcionQuote.DolarBlue,
                    Valor = dolarBlue.Precio,
                    Tooltip = dolarBlue.MensajeError
                });

                // Dolar MEP
                try
                {
                    quotes.Add(new Quotes
                    {
                        Tipo = ETipoQuote.DOLAR,
                        Titulo = TitulosQuote.DolarMepCI,
                        Descripcion = DescripcionQuote.DolarMepCI,
                        Valor = Math.Round((bonoGD30.MarketData.LA.Price/bonoGD30D.MarketData.LA.Price), 2)
                    });
                }
                catch
                {
                    quotes.Add(new Quotes
                    {
                        Tipo = ETipoQuote.DOLAR,
                        Titulo = TitulosQuote.DolarMepCI,
                        Descripcion = DescripcionQuote.DolarMepCI,
                        Valor = 0,
                        Tooltip = new TooltipMessage()
                        {
                            Tipo = "error",
                            Mensaje = "Error en los bonos GD30/GD30D"
                        }
                    });
                }

                // Dolar CCL 
                try
                {
                    quotes.Add(new Quotes
                    {
                        Tipo = ETipoQuote.DOLAR,
                        Titulo = TitulosQuote.DolarCCLCI,
                        Descripcion = DescripcionQuote.DolarCCLCI,
                        Valor = Math.Round((bonoGD30.MarketData.LA.Price / bonoGD30C.MarketData.LA.Price), 2)
                    });
                }
                catch
                {
                    quotes.Add(new Quotes
                    {
                        Tipo = ETipoQuote.DOLAR,
                        Titulo = TitulosQuote.DolarCCLCI,
                        Descripcion = DescripcionQuote.DolarCCLCI,
                        Valor = 0,
                        Tooltip = new TooltipMessage()
                        {
                            Tipo = "error",
                            Mensaje = "Error en los bonos GD30/GD30C"
                        }
                    });

                }

                // Dolares MEP 48Hs
                try
                {
                    quotes.Add(new Quotes
                    {
                        Tipo = ETipoQuote.DOLAR,
                        Titulo = TitulosQuote.DolarMep48Hs,
                        Descripcion = DescripcionQuote.DolarMep48Hs,
                        Valor = Math.Round((bono48Horas.MarketData.LA.Price / bono48MEP.MarketData.LA.Price), 2)
                    });
                } 
                catch
                {
                    quotes.Add(new Quotes
                    {
                        Tipo = ETipoQuote.DOLAR,
                        Titulo = TitulosQuote.DolarMep48Hs,
                        Descripcion = DescripcionQuote.DolarMep48Hs,
                        Valor = 0,
                        Tooltip = new TooltipMessage()
                        {
                            Tipo = "error",
                            Mensaje = "Error en los bonos GD30/GD30D 48HS"
                        }
                    });
                }

                // Dolares MEP 48Hs
                double DolarMepCCL48Hs = 0;
                try
                {
                    DolarMepCCL48Hs =  Math.Round((bono48Horas.MarketData.LA.Price / bono48CCL.MarketData.LA.Price), 2);
                    quotes.Add(new Quotes
                    {
                        Tipo = ETipoQuote.DOLAR,
                        Titulo = TitulosQuote.DolarCCL48Hs,
                        Descripcion = DescripcionQuote.DolarCCL48Hs,
                        Valor = DolarMepCCL48Hs
                    });
                } 
                catch
                {
                    quotes.Add(new Quotes
                    {
                        Tipo = ETipoQuote.DOLAR,
                        Titulo = TitulosQuote.DolarCCL48Hs,
                        Descripcion = DescripcionQuote.DolarCCL48Hs,
                        Valor = 0,
                        Tooltip = new TooltipMessage()
                        {
                            Tipo = "error",
                            Mensaje = "Error en los bonos GD30/GD30c 48HS"
                        }
                    });
                }

                // Dolar CCL CEDEARs
                quotes.Add(new Quotes
                {
                    Tipo = ETipoQuote.DOLAR,
                    Titulo = TitulosQuote.DolarCCLCEDEARS,
                    Descripcion = dolarCCLCedear.DescripcionTitulo,
                    Valor = dolarCCLCedear.UltimoPrecio
                });

                // Canje
                quotes.Add(new Quotes
                {
                    Tipo = ETipoQuote.CANJE,
                    Titulo = TitulosQuote.Canje,
                    Descripcion = DescripcionQuote.Canje,
                    Valor = Math.Round(((quotes.Find(x => x.Titulo == TitulosQuote.DolarCCLCI).Valor / quotes.Find(x => x.Titulo == TitulosQuote.DolarMepCI).Valor) - 1) * 100, 2)
                });

                // Cauciones
                quotes.AddRange(cauciones.Select(x => new Quotes
                {
                    Tipo = ETipoQuote.CAUCION,
                    Subtipo = x.Plazo == 7 ? ESubTipoQuote.PLAZO_LEJANO : ESubTipoQuote.PLAZO_CERCANO,
                    Titulo = $"Tasa caución {x.Plazo} {(x.Plazo == 1 ? "día" : "días")}",
                    Descripcion = $"Caución en pesos con plazo de {x?.Plazo} días",
                    Valor = x != null ? x.TasaPromedio : 0,
                    Tooltip = x?.Tooltip
                }));

                // Merval
                quotes.Add(new Quotes
                {
                    Tipo = ETipoQuote.INDICE,
                    Titulo = TitulosQuote.Merval,
                    Descripcion = DescripcionQuote.Merval,
                    Valor = merval.Price
                });

                quotes.Add(new Quotes
                {
                    Tipo = ETipoQuote.INDICE,
                    Titulo = TitulosQuote.MervalUsd,
                    Descripcion = DescripcionQuote.MervalUsd,
                    Valor = Math.Round((merval.Price / DolarMepCCL48Hs), 2)
                });

                // ACCIONES
                //quotes.AddRange(detallesCotizaciones.Select(x => new Quotes
                //{
                //    Tipo = ETipoQuote.ACCION,
                //    Titulo = x.Simbolo == ETFAccionType.QQQ ? "NASDAQ" : x.Simbolo == ETFAccionType.DIA ? "DOW" : x.Simbolo,
                //    Descripcion = x.Simbolo,
                //    Valor = x.UltimoPrecio
                //}));

                //
                // Chequear valores NAN en propiedad "UltimoOperado"
                //
                foreach (Quotes quote in quotes)
                {

                    if (double.IsInfinity(quote.Valor))
                    {
                        quote.Valor = 0;
                        quote.Tooltip = new TooltipMessage()
                        {
                            Tipo = "error",
                            Mensaje = "Se obtuvo un valor inválido (infinito)"
                        };
                        continue;
                    }

                    if ((quote.Tipo == ETipoQuote.DOLAR || quote.Tipo == ETipoQuote.CANJE) && Double.IsNaN((double)quote.Valor))
                    {
                        quote.Valor = 0;
                        quote.Tooltip = new TooltipMessage()
                        {
                            Tipo = "error",
                            Mensaje = "Se obtuvo un valor inválido (NaN)"
                        };
                        continue;
                    }

                    if ((quote.Tipo == ETipoQuote.INDICE) && quote.Valor == 0)
                    {
                        quote.Tooltip = new TooltipMessage()
                        {
                            Tipo = "alert",
                            Mensaje = "Índice no disponible momentáneamente"
                        };
                        continue;
                    }

                    if ((quote.Tipo == ETipoQuote.INDICE) && Double.IsNaN((double)quote.Valor))
                    {
                        quote.Valor = 0;
                        quote.Tooltip = new TooltipMessage()
                        {
                            Tipo = "error",
                            Mensaje = "Se obtuvo un valor inválido (NaN)"
                        };
                        continue;
                    }

                    if((quote.Valor == 0 && quote.Tooltip == null))
                    {
                        quote.Tooltip = new TooltipMessage()
                        {
                            Tipo = "error",
                            Mensaje = "No se obtuvo un valor"
                        };
                    }

                }

                //
                // Devolver lista de cotizaciones
                //
                return quotes;

                // TODO: Titulos con Enum
            }
            catch (Exception ex)
            {
                _logger.LogError("Error in GetQuotesAsync", ex.Message);
            }

            return quotes;
        }

        public async Task<bool> InformQuotesAsync(List<Quotes> quotes, string endpoint)
        {
            var succeeded = false;

            try
            {
                var url = _cobraApi.Value.Url.AppendPath("Quotation", endpoint);

                var informQuoteRequest = await url
                    .PostJsonToUrlAsync(quotes,
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
        #endregion

        #region PrivateMethods
        protected async Task CheckTokenOmsAsync()
        {
            if (string.IsNullOrEmpty(OmsToken))
            {
                OmsToken = await GetTokenOmsAsync();
            }
        }

        private async Task<string> GetTokenOmsAsync()
        {
            var token = string.Empty;
            try
            {
                var url = $"{_omsAPI.Value.ServerUrl}{ApiUrl.Token}";
                HttpClient client = HttpClientFactoryExtensions.CreateClient(_httpClientFactory);
                var password = AesManager.GetPassword(_omsAPI.Value.Password, _configuration.GetSection("SecretKey").Value);
                var oauthBasePassword = AesManager.GetPassword(_omsAPI.Value.oauthBasePassword, _configuration.GetSection("SecretKey").Value);

                client.BaseAddress = new Uri(url);
                var dataPost = new StringContent($"grant_type=password&username={_omsAPI.Value.Username}&password={password}",
                                                Encoding.UTF8,
                                                "application/x-www-form-urlencoded");
                client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue(
                    "Basic", Convert.ToBase64String(
                        Encoding.ASCII.GetBytes($"{_omsAPI.Value.oauthBaseUsername}:{oauthBasePassword}")
                ));


                var response = await client.PostAsync(url, dataPost);
                var content = await response.Content.ReadAsStringAsync();
                var result = System.Text.Json.JsonSerializer.Deserialize<Token>(content);
                return result?.access_token;

            }
            catch (Exception ex)
            {
                _logger.LogError("GetTokenAsync() - Error while getting token from OMS", ex.Message);
            }
            return token;
        }

        private async Task<string> InvertirOnlineLoginAsync()
        {
            try
            {
                _logger.LogDebug("Starting call to InvertirOnlineLoginAsync...");

                var url = _invertirOnline.Value.Url.AppendPath("token");

                var loginRequest = await url.PostToUrlAsync(new
                {
                    username = _invertirOnline.Value.User,
                    password = AesManager.GetPassword(_invertirOnline.Value.Pass, _configuration.GetSection("SecretKey").Value),
                    grant_type = "password"
                });

                var loginResponse = loginRequest.FromJson<Token>();

                return loginResponse.access_token;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error in InvertirOnlineLoginAsync");

                return string.Empty;
            }
        }

        private CotizacionDetalle GetDolarCCLCedear(List<CotizacionDetalle> qUSA, List<CotizacionDetalle> qCedears)
        {
            var cotizacionesValores = new List<CotizacionDetalle>();
            try
            {
                foreach (var u in qUSA)
                {
                    foreach (var ar in qCedears)
                    {
                        if (u.Simbolo == ar.Simbolo)
                        {
                            cotizacionesValores.Add(new CotizacionDetalle
                            {
                                Simbolo = ar.Simbolo,
                                UltimoPrecio = Math.Round(Convert.ToDouble((ar.UltimoPrecio / (u.UltimoPrecio)) * GetRatioBySymbol(ar.Simbolo), new CultureInfo("es-US")),2)
                            });

                        }
                    }
                }

                var valoresFinales = cotizacionesValores.Where(x => x != cotizacionesValores.MaxBy(v => v.UltimoPrecio) && x != cotizacionesValores.MinBy(v => v.UltimoPrecio)).ToList();

                var promedio = Math.Round((valoresFinales.Sum(x => Convert.ToDouble(x.UltimoPrecio, new CultureInfo("es-US")) / valoresFinales.Count())),2);

                return new CotizacionDetalle
                {
                    UltimoPrecio = promedio
                };
            }
            catch (Exception ex)
            {

                _logger.LogError("Error in GetDolarCCLCedear", ex.Message);
            }

            return new CotizacionDetalle();

        }
        public double GetRatioBySymbol(string symbol)
        {
            if (string.IsNullOrEmpty(symbol))
            { return 0; }

            return Int32.Parse(_configuration.GetSection($"QuotationConfiguration:QuotationRatios:{symbol}").Value);
        }

        private static class RequestUriUtil
        {
            public static string GetUriWithQueryString(string requestUri,
                Dictionary<string, string> queryStringParams)
            {
                bool startingQuestionMarkAdded = false;
                var sb = new StringBuilder();
                sb.Append(requestUri);
                foreach (var parameter in queryStringParams)
                {
                    if (parameter.Value == null)
                    {
                        continue;
                    }

                    sb.Append(startingQuestionMarkAdded ? '&' : '?');
                    sb.Append(parameter.Key);
                    sb.Append('=');
                    sb.Append(parameter.Value);
                    startingQuestionMarkAdded = true;
                }
                return sb.ToString();
            }
        }
        #endregion

    }
}