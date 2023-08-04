using System;
using System.Collections.Generic;
using System.Data;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using ExcelDataReader;
using Ganss.Excel;
using Hangfire;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Options;
using Newtonsoft.Json;
using nordelta.cobra.webapi.Configuration;
using nordelta.cobra.webapi.Controllers.Helpers;
using nordelta.cobra.webapi.Controllers.ViewModels;
using nordelta.cobra.webapi.Models;
using nordelta.cobra.webapi.Repositories.Contracts;
using nordelta.cobra.webapi.Services.Contracts;
using nordelta.cobra.webapi.Utils;
using RestSharp;
using Log = Serilog.Log;
using Monitoreo = Nordelta.Monitoreo;


namespace nordelta.cobra.webapi.Services
{
    public class ExchangeRateFilesService : IExchangeRateFilesService
    {
        private readonly IConfiguration _configuration;
        private readonly IExchangeRateFileRepository _exchangeRateFileRepository;
        private readonly IOptionsMonitor<ApiServicesConfig> _apiServicesConfig;
        private readonly IRestClient _restClient;
        private readonly INotificationService _notificationService;
        private readonly IArchivoDeudaRepository _archivoDeudaRepository;
        private readonly IHolidaysService _holidaysService;
        private readonly IBackgroundJobClient _backgroundJobClient;
        private readonly List<ReportQuotationsConfiguration> _reportQuotationsToSgc;
        private readonly ServiciosMonitoreadosConfiguration _servicios;

        public ExchangeRateFilesService(
            IConfiguration configuration,
            IExchangeRateFileRepository exchangeRateFileRepository,
            IOptionsMonitor<ApiServicesConfig> options,
            IRestClient restClient,
            INotificationService notificationService,
            IArchivoDeudaRepository archivoDeudaRepository,
            IHolidaysService holidaysService,
            IBackgroundJobClient backgroundJobClient,
            IOptions<ServiciosMonitoreadosConfiguration> servicesMonConfig,
            IOptionsMonitor<List<ReportQuotationsConfiguration>> reportQuotationsConfig
        )
        {
            _configuration = configuration;
            _exchangeRateFileRepository = exchangeRateFileRepository;
            _apiServicesConfig = options;
            _restClient = restClient;
            _notificationService = notificationService;
            _archivoDeudaRepository = archivoDeudaRepository;
            _holidaysService = holidaysService;
            _backgroundJobClient = backgroundJobClient;
            _reportQuotationsToSgc = reportQuotationsConfig.Get(ReportQuotationsConfiguration.ReportToSGC);
            _servicios = servicesMonConfig.Value;
        }

        [JobDisplayName("PublicacionTasaDeCambio")]
        public void ProcessAllFiles()
        {
            string filePath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "ExchangeRateFiles");
            if (!Directory.Exists(filePath))
            {
                Directory.CreateDirectory(filePath);
            }
            string[] files = Directory.GetFiles(filePath);
            foreach (string file in files)
            {
                ExchangeRateFile newFile = ProcessPaymentFile(file);

                ExchangeRateFile existingFile = _exchangeRateFileRepository.GetByFileName(newFile.FileName);
                if (existingFile == null && !string.IsNullOrEmpty(newFile.UvaExchangeRate))
                {
                    _exchangeRateFileRepository.Add(newFile);
                }
            }
        }

        public double GetLastUsdFromDetalleDeuda()
        {
            // TODO: Remove startsWith workaround and use some currency discriminator for the exchangeRate 
            var filteredList = _archivoDeudaRepository.GetLastDetalleDeudas()
                .Where(x => x.CodigoMonedaTc != null && x.CodigoMonedaTc.Equals(ExchangeRateCurrency.USD));
            var result = filteredList.Any() ? filteredList.Max(x => Convert.ToDouble(x.ObsLibreTercera, new CultureInfo("en-US"))) : 0;

            return result;
        }

        public ExchangeRateFile GetLastExchangeRateFile()
        {
            return _exchangeRateFileRepository.GetLastExchangeRateFileAvailable();
        }

        public async Task<List<BonoConfig>> GetEspeciesAsync()
        {
            var result = new List<BonoConfig>();
            try
            {
                _restClient.BaseUrl = new Uri(_apiServicesConfig.Get(ApiServicesConfig.QuotationServiceApi).Url);
                RestRequest request = new RestRequest("/Quotation/GetBonosConfiguration", Method.GET)
                {
                    RequestFormat = DataFormat.Json
                };
                request.AddHeader("token", _apiServicesConfig.Get(ApiServicesConfig.QuotationServiceApi).Token);

                IRestResponse response = Task.Run(async () => await _restClient.ExecuteAsync(request))
                    .ConfigureAwait(false)
                    .GetAwaiter()
                    .GetResult();


                if (response.IsSuccessful)
                {
                    result = JsonConvert.DeserializeObject<List<BonoConfig>>(response.Content);

                    return result;

                    Log.Information($"GetEspeciesAsync a {request} de microservicio quotation:", request.Resource);
                }
                else
                {
                    Log.Error($"GetEspeciesAsync a {request} de microservicio quotation:", response);
                }
            }
            catch (Exception ex)
            {
                Log.Error("An Error was found in ExchangeRateService > GetQuotationsSourceAsync  > Error", ex.Message);
            }

            return result;
        }

        public async Task<List<QuotationExternal>> GetSourceQuotationsAsync(DateTime date, IEnumerable<string> quotes)
        {
            var result = new List<QuotationExternal>();
            var dateNow = LocalDateTime.GetDateTimeNow();
            var quotationBcra = new QuotationExternal();
            if (quotes.Any(x => x.Contains("BCRA")))
            {

                quotationBcra = GetFromCbraHistorico(date);
                result.Add(quotationBcra);

            }

            if (quotationBcra != null)
            {
                quotes = quotes.Where(x => !x.Contains("BCRA")).ToList();
            }
            try
            {
                _restClient.BaseUrl = new Uri(_apiServicesConfig.Get(ApiServicesConfig.QuotationServiceApi).Url);
                RestRequest request = new RestRequest("/Quotation/GetSourceQuote", Method.POST)
                {
                    RequestFormat = DataFormat.Json
                };
                request.AddHeader("token", _apiServicesConfig.Get(ApiServicesConfig.QuotationServiceApi).Token);
                var dateString = date.ToString("o");
                request.AddParameter("date", dateString, ParameterType.QueryString);
                request.AddJsonBody(quotes);

                IRestResponse response = Task.Run(async () => await _restClient.ExecuteAsync(request))
                    .ConfigureAwait(false)
                    .GetAwaiter()
                    .GetResult();


                if (response.IsSuccessful)
                {
                    result.AddRange(JsonConvert.DeserializeObject<List<QuotationExternal>>(response.Content));

                    return result;

                    Log.Information($"GetQuotationsSourceAsync a {request} de microservicio quotation:", request.Resource);
                }
                else
                {
                    Log.Error($"GetQuotationsSourceAsync a {request} de microservicio quotation:", response);
                }

            }
            catch (Exception ex)
            {
                Log.Error("An Error was found in ExchangeRateService > GetQuotationsSourceAsync  > Error", ex.Message);
            }
            return result;
        }


        public void PublishQuotationByBono(IEnumerable<Bono> bonos)
        {
            Bono newBono = new Bono();
            var titles = bonos.Select(x => x.Title).ToList();
            newBono.Title = string.Join(",", titles);

            try
            {
                _exchangeRateFileRepository.AddBonoConfig(newBono);

            }
            catch (Exception ex)
            {
                Log.Error("ERROR publishing quotation by bono: {@err}", ex);
            }

        }

        public List<Bono> GetLastBonosConfig()
        {
            var bono = _exchangeRateFileRepository.GetLastBonosConfig();
            string[] bonosTitles = bono?.Title.Split(",");
            var listaBonos = new List<Bono>();
            if (bonosTitles != null)
            {
                foreach (var title in bonosTitles)
                {
                    listaBonos.Add(new Bono { Title = title });
                }

            }
            return listaBonos;
        }

        public List<QuotationExternal> CheckQuotationsExists(List<QuotationExternal> quotations, DateTime date)
        {
            var generatedQuotations = GenerateQuotations(quotations, date);

            try
            {
                List<QuotationExternal> quotationList = new List<QuotationExternal>();
                if (generatedQuotations != null && generatedQuotations.Any())
                {
                    foreach (var quotation in generatedQuotations)
                    {
                        QuotationExternal quotationObj = JsonConvert.DeserializeObject<QuotationExternal>(JsonConvert.SerializeObject(quotation));
                        quotationObj.StoredInDb = _exchangeRateFileRepository.CheckQuotationExists(quotationObj);
                        quotationList.Add(quotationObj);
                    }
                }

                return quotationList;
            }
            catch (Exception ex)
            {
                Serilog.Log.Error(@"Exception: Error getting bonos: {@error}", ex);
            }
            return quotations;

        }

        private ExchangeRateFile ProcessPaymentFile(string filePath)
        {
            ExchangeRateFile exchangeRateFile = new ExchangeRateFile();

            if (File.Exists(filePath))
            {
                exchangeRateFile.FileName = Path.GetFileNameWithoutExtension(filePath);
                exchangeRateFile.TimeStamp = LocalDateTime.GetDateTimeNow().ToString(CultureInfo.InvariantCulture);
                string[] lines = File.ReadLines(filePath).ToArray();
                if (lines.Any())
                {
                    exchangeRateFile.UvaExchangeRate = lines[0];
                }
            }

            return exchangeRateFile;
        }

        public List<dynamic> GetAllQuotations(string type)
        {
            return _exchangeRateFileRepository.GetAllQuotations(type);
        }

        public List<QuotationViewModel> GetQuotationTypes()
        {
            return _exchangeRateFileRepository.GetQuotationTypes();
        }

        public List<string> GetSourceTypes()
        {
            var excludedSources = new[] {
                EQuotationSource.CAMARCO,
                EQuotationSource.MANUAL,
                EQuotationSource.RAVA,
                EQuotationSource.BYMA,
                EQuotationSource.ARCHIVO_PUBLICACION,
                EQuotationSource.OLAP,
                EQuotationSource.COBRA };

            return Enum.GetNames(typeof(EQuotationSource))
                                 .Where(x => !excludedSources.Contains((EQuotationSource)Enum.Parse(typeof(EQuotationSource), x)))
                                 .ToList();
        }

        public dynamic GetLastQuotation(string type)
        {
            return _exchangeRateFileRepository.GetLastQuotation(type);
        }

        public dynamic GetCurrentQuotation(string type, bool lastQuote = false)
        {
            if (type == "UVA")
            {
                return GetUvaQuotation();
            }
            else
            {
                return _exchangeRateFileRepository.GetCurrentQuotation(type, lastQuote);
            }
        }

        private Quotation GetUvaQuotation()
        {
            //Si no hay cotización devuelve una instancia vacía de UVA
            var uvaQuotation = _exchangeRateFileRepository.GetCurrentQuotation("UVA");
            if (uvaQuotation.Valor != 0)
                return uvaQuotation;
            var uvaFromFile = _exchangeRateFileRepository.GetLastExchangeRateFileAvailable();
            //Si no hay uva del archivo devuelvo la instancia vacía de UVA obtenida anteriormente
            if (uvaFromFile != null)
            {
                var uvaDate = DateTime.ParseExact(uvaFromFile.TimeStamp, "MM/dd/yyyy HH:mm:ss", CultureInfo.InvariantCulture);
                return new UVA
                {
                    Source = EQuotationSource.ARCHIVO_PUBLICACION,
                    Valor = double.Parse(uvaFromFile.UvaExchangeRate, CultureInfo.InvariantCulture),
                    Description = uvaFromFile.FileName,
                    FromCurrency = "ARS",
                    ToCurrency = "UVA",
                    EffectiveDateFrom = uvaDate,
                    EffectiveDateTo = uvaDate,
                    UploadDate = uvaDate
                };
            }
            else
                return uvaQuotation;
        }

        public dynamic AddQuotation(string quotationType, dynamic quotation, User user, EQuotationSource loadType)
        {
            var result = _exchangeRateFileRepository.AddQuotation(quotationType, quotation, user, loadType);
            try
            {
                if (result != null)
                {
                    int minutesForCancellation = Int32.Parse(_configuration.GetSection("QuotationCancellationTimeInMinutes").Value);
                    int quotationId = result.Id;
                    if (quotationType == "DolarMEP")
                    {
                        _backgroundJobClient.Enqueue(() => NotifyQuotationCancellation(quotationId));
                    }

                    if (loadType == EQuotationSource.MANUAL && (quotationType == "CAC" || quotationType == "DolarMEP"))
                    {
                        const bool fromManual = true;
                        _backgroundJobClient.Enqueue(() => GetUSDCAC(fromManual));
                    }
                    _backgroundJobClient.Enqueue(() => InformQuotations(new List<int> { quotationId }, minutesForCancellation));
                }
            }
            catch (Exception ex)
            {
                Log.Error("ERROR al informar cotización a SGF: {@error}", ex);
                throw;
            }
            return result;
        }

        public dynamic AddQuotationsAndInform(dynamic data)
        {
            List<QuotationExternal> generatedQuotations = new List<QuotationExternal>();
            List<QuotationExternal> quotationsOrigen = new List<QuotationExternal>();

            var filteredDate = data.date.ToObject<DateTime>();
            foreach (var quotation in data.generatedQuotations)
            {
                QuotationExternal quotationExternal = quotation.ToObject<QuotationExternal>();
                generatedQuotations.Add(quotationExternal);
            }

            foreach (var quotation in data.quotationsOrigen)
            {
                QuotationExternal quotationExternal = quotation.ToObject<QuotationExternal>();
                quotationsOrigen.Add(quotationExternal);
            }

            List<string> systems = new List<string>();
            foreach (var system in data.systems)
            {
                string systemStr = system.ToObject<string>();
                systems.Add(systemStr);
            }

            //Generamos las cotizaciones con la fecha cona la que se filtraron.
            var getQuotations = GenerateQuotations(quotationsOrigen, filteredDate);

            //Se genera una lista de cotizaciones que no estan registradas en base.

            var quotesToAdd = new List<dynamic>();

            foreach (var item in generatedQuotations)
            {
                //Corrobaramos que las cotizaciones no registradas coincidan con las generadas
                foreach (var quote in getQuotations)
                {
                    if (item.Description == quote.Description && item.RateType == quote.RateType && item.Source == quote.Source)
                    {
                        quote.Id = 0;
                        quote.StoredInDb = item.StoredInDb;
                        quotesToAdd.Add(quote);
                    }
                }
            }

            try
            {
                var result = AddQuotations(quotesToAdd, new User { Id = "ServiceWorker" }, systems);
                return result;
            }
            catch (Exception ex)
            {
                Log.Error("ERROR al guardar e informar cotizaciones: {@error}", ex);
                throw;
            }
        }

        public List<dynamic> AddQuotations(dynamic quotations, User user)
        {
            var IdsOk = new List<int>();
            var quotationsOk = new List<dynamic>();
            try
            {
                int minutesForCancellation = Int32.Parse(_configuration.GetSection("QuotationCancellationTimeInMinutes").Value);
                foreach (var quotation in quotations)
                {
                    var result = _exchangeRateFileRepository.AddQuotation(null, quotation, user, quotation.Source);

                    if (result != null)
                    {
                        int quotationId = result.Id;

                        quotationsOk.Add(result);
                        IdsOk.Add(quotationId);
                        //_backgroundJobClient.Enqueue(() => NotifyQuotationCancellation(quotationId));
                    }
                }
                _backgroundJobClient.Enqueue(() => InformQuotations(IdsOk, minutesForCancellation));
            }
            catch (Exception ex)
            {
                Log.Error("Error creating quotations: {@error}", ex);
                throw;
            }
            return quotationsOk;
        }

        public List<dynamic> AddQuotations(dynamic quotations, User user, List<string> systems)
        {
            var quotationsOk = new List<dynamic>();
            try
            {
                int minutesForCancellation = Int32.Parse(_configuration.GetSection("QuotationCancellationTimeInMinutes").Value);


                foreach (var quotation in quotations)
                {
                    if (!quotation.StoredInDb)
                    {
                        var result = _exchangeRateFileRepository.AddQuotation(null, quotation, user, quotation.Source);

                    }
                    quotationsOk.Add(quotation);
                }
                InformQuotations(quotationsOk, minutesForCancellation, systems);

            }
            catch (Exception ex)
            {
                Log.Error("Error creating quotations: {@error}", ex);
                throw;
            }
            return quotationsOk;
        }


        public List<dynamic> AddCryptocurrencyQuotations(dynamic quotations, User user)
        {
            var IdsOk = new List<int>();
            var quotationsOk = new List<dynamic>();
            try
            {
                foreach (var quotation in quotations)
                {
                    var result = _exchangeRateFileRepository.AddQuotation(null, quotation, user, quotation.Source);
                    if (result != null)
                    {
                        int quotationId = result.Id;
                        quotationsOk.Add(result);
                        IdsOk.Add(quotationId);
                    }
                }
                _backgroundJobClient.Enqueue(() => InformQuotations(IdsOk, 0));
            }
            catch (Exception ex)
            {
                Log.Error("Error creating cryptocurrency quotations: {@error}", ex);
                throw;
            }
            return quotationsOk;
        }

        public void NotifyQuotationCancellation(int quotationId)
        {
            _notificationService.NotifyQuotationCancellation(quotationId);
        }

        public void InformQuotations(List<int> IdsOk, int minutesForCancellation)
        {
            BackgroundJob.Schedule(() => InformQuotations(IdsOk), TimeSpan.FromMinutes(minutesForCancellation));
        }

        public void InformQuotations(List<dynamic> quotations, int minutesForCancellation, List<string> systems)
        {
            BackgroundJob.Schedule(() => InformQuotations(quotations, systems), TimeSpan.FromMinutes(minutesForCancellation));
        }

        private List<Quotation> AddQuotations(List<Quotation> quotations)
        {
            try
            {
                return _exchangeRateFileRepository.AddQuotations(quotations);
            }
            catch (Exception ex)
            {
                Serilog.Log.Error("ERROR saving quotations: {@err}", ex);
                return null;
            }
        }

        public bool CancelQuotation(int quotationId)
        {
            var quotation = _exchangeRateFileRepository.GetQuotationById(quotationId);
            if (quotation != null)
            {
                int minutesForCancellation = Int32.Parse(_configuration.GetSection("QuotationCancellationTimeInMinutes").Value);
                TimeSpan ts = LocalDateTime.GetDateTimeNow() - quotation.UploadDate;
                if (ts.TotalMinutes <= minutesForCancellation)
                {
                    _exchangeRateFileRepository.CancelQuotation(quotation);
                    return true;
                }
                else
                    return false;
            }
            else
                return false;
        }
        public bool CheckDolarMepJobWasExecuted()
        {
            return _exchangeRateFileRepository.CheckDolarMepJobWasExecuted();
        }

        public bool ExecuteGetDolarMepAsync()
        {
            bool success = false;
            try
            {
                _restClient.BaseUrl = new Uri(_apiServicesConfig.Get(ApiServicesConfig.QuotationServiceApi).Url);
                RestRequest request = new RestRequest("/Quotation/GetDolarMep", Method.GET);

                request.AddHeader("token", _apiServicesConfig.Get(ApiServicesConfig.QuotationServiceApi).Token);

                IRestResponse response = Task.Run(async () => await _restClient.ExecuteAsync(request))
                    .ConfigureAwait(false)
                    .GetAwaiter()
                    .GetResult();

                if (response.IsSuccessful)
                {
                    Log.Information($"ExecuteGetDolarMepAsync a {request.Resource} de quotation api:");
                    return response.IsSuccessful;
                }
                else
                {
                    Log.Error($"Fallo solicitud: ExecuteGetDolarMepAsync a {request.Resource} de quotation api");
                    return success;
                }
            }
            catch (Exception ex)
            {
                Log.Error("An Error was found in ExchangeRateService > ExecuteGetDolarMepAsync  > Error", ex.Message);
            }

            return success;
        }

        public void InformQuotations(List<int> quotationIds)
        {
            //Informa la nueva cotización a Oracle y si coincide con los tc en configuracion también a SGC
            var quotations = _exchangeRateFileRepository.GetQuotationsByIds(quotationIds);

            if (quotations.Any())
            {
                var dataForSGC = new List<QuotationOracleViewModel>();
                var dataForOracle = new List<QuotationOracleViewModel>();

                foreach (var quotation in quotations)
                {
                    // ORACLE
                    dataForOracle.Add(new QuotationOracleViewModel
                    {
                        DateFrom = quotation.EffectiveDateFrom,
                        DateTo = quotation.EffectiveDateTo,
                        Rate = quotation.Valor,
                        FromCurrency = quotation.FromCurrency,
                        ToCurrency = quotation.ToCurrency,
                        RateType = quotation.RateType
                    });

                    // SGC
                    if (_reportQuotationsToSgc.Any(x => x.Discriminator == quotation.GetType().Name && x.RateTypes.Contains(quotation.RateType)))
                    {
                        dataForSGC.Add(new QuotationOracleViewModel
                        {
                            DateFrom = quotation.EffectiveDateFrom,
                            DateTo = quotation.EffectiveDateTo.Date,
                            Rate = quotation.Valor,
                            FromCurrency = quotation.FromCurrency,
                            ToCurrency = quotation.ToCurrency,
                            RateType = quotation.RateType
                        });
                    }
                };

                // Add USD MEP Corporate
                if (dataForOracle.Any(x => x.RateType == RateTypes.UsdMEP))
                {
                    var mepCorporate = dataForOracle.First(x => x.RateType == RateTypes.UsdMEP).DeepCopy();
                    mepCorporate.RateType = RateTypes.Corporate;
                    dataForOracle.Add(mepCorporate);
                }

                // Informar a Oracle via SGF
                ReportToOracleBySGF(dataForOracle);

                // Add USD MEP Corporate
                if (dataForSGC.Any(x => x.RateType == RateTypes.UsdMEP))
                {
                    var mepCorporate = dataForSGC.First(x => x.RateType == RateTypes.UsdMEP).DeepCopy();
                    mepCorporate.RateType = "USD MEP Corporate";
                    dataForSGC.Add(mepCorporate);
                }

                // Informar a SGC
                if (dataForSGC.Any())
                    ReportToSGC(dataForSGC);

                Log.Information("InformNewQuotation to SGC : {infoSGC} \n InformNewQuotation to ORACLE : {oracleSGC}", JsonConvert.SerializeObject(dataForSGC), JsonConvert.SerializeObject(dataForOracle));
            }
            else
            {
                Log.Information("InformNewQuotation: Las cotizaciones {@quotations} no se informaron debido a que fueron canceladas", quotationIds);
            }
        }

        public void InformQuotations(List<dynamic> quotations, List<string> systemsToInform)
        {
            //Informa la nueva cotización a Oracle y si coincide con los tc en configuracion también a SGC

            if (quotations.Any())
            {
                var dataForOracle = new List<QuotationOracleViewModel>();
                var dataForSGC = new List<QuotationOracleViewModel>();

                foreach (var quotation in quotations)
                {
                    if (systemsToInform.Any(x => x.Contains("SGF")))
                    {
                        // ORACLE
                        dataForOracle.Add(new QuotationOracleViewModel
                        {
                            DateFrom = quotation.EffectiveDateFrom,
                            DateTo = quotation.EffectiveDateTo,
                            Rate = quotation.Valor,
                            FromCurrency = quotation.FromCurrency,
                            ToCurrency = quotation.ToCurrency,
                            RateType = quotation.RateType
                        });
                    }
                    if (systemsToInform.Any(x => x.Contains("SGC")))
                    {
                        // SGC
                        if (_reportQuotationsToSgc.Any(x => x.Discriminator == quotation.GetType().Name && x.RateTypes.Contains(quotation.RateType)))
                        {
                            dataForSGC.Add(new QuotationOracleViewModel
                            {
                                DateFrom = quotation.EffectiveDateFrom,
                                DateTo = quotation.EffectiveDateTo.Date,
                                Rate = quotation.Valor,
                                FromCurrency = quotation.FromCurrency,
                                ToCurrency = quotation.ToCurrency,
                                RateType = quotation.RateType
                            });
                        }
                    }
                };

                if (dataForOracle.Any())
                {
                    // Add USD MEP Corporate
                    if (dataForOracle.Any(x => x.RateType == RateTypes.UsdMEP))
                    {
                        var mepCorporate = dataForOracle.First(x => x.RateType == RateTypes.UsdMEP).DeepCopy();
                        mepCorporate.RateType = RateTypes.Corporate;
                        dataForOracle.Add(mepCorporate);
                    }

                    // Informar a Oracle via SGF
                    ReportToOracleBySGF(dataForOracle);
                }

                if (dataForSGC.Any())
                {
                    // Add USD MEP Corporate
                    if (dataForSGC.Any(x => x.RateType == RateTypes.UsdMEP))
                    {
                        var mepCorporate = dataForSGC.First(x => x.RateType == RateTypes.UsdMEP).DeepCopy();
                        mepCorporate.RateType = "USD MEP Corporate";
                        dataForSGC.Add(mepCorporate);
                    }

                    // Informar a SGC
                    if (dataForSGC.Any())
                        ReportToSGC(dataForSGC);
                }
                Log.Information("InformNewQuotation to SGC : {infoSGC} \n InformNewQuotation to ORACLE : {oracleSGC}", JsonConvert.SerializeObject(dataForSGC), JsonConvert.SerializeObject(dataForOracle));
            }
            else
            {
                Log.Information("InformNewQuotation: Las cotizaciones {@quotations} no se informaron debido a que fueron canceladas", quotations);
            }
        }

        public bool CheckCacExists(string webDate)
        {
            return _exchangeRateFileRepository.CheckCacExists(webDate);
        }

        public void GetUSDCAC(bool fromManual)
        {
            var MsgOkCacUsd = "Se obtuvo correctamente la cotización CAC USD";
            var MsgCriticalCacUsd = "Ocurrió un error al intentar obtener cotización CAC USD";

            var MsgOkCacUsdCorp = "Se obtuvo correctamente la cotización CAC USD Corporate";
            var MsgCriticalCacUsdCorp = "Ocurrió un error al intentar obtener cotización CAC USD Corporate";

            List<CacConfig> cacConfigs = new List<CacConfig>();
            //_configuration.GetSection("ItemsUsdCac").Bind(cacConfigs);
            var dateNow = LocalDateTime.GetDateTimeNow();
            try
            {
                if (_holidaysService.IsAHoliday(dateNow) || _holidaysService.IsWeekend(dateNow)) return;
                const bool lastQuote = true;
                var currentCAC = GetCurrentQuotation("CAC", lastQuote);
                var currentMEP = GetCurrentQuotation("DolarMEP", lastQuote);
                if (currentCAC.Valor > 0 && currentMEP.Valor > 0)
                {

                    List<Quotation> quotations = new List<Quotation>();
                    var cacUsd = new CACUSD();
                    var cacUsdCorp = new CACUSDCorporate();

                    var CacUsdValue = Math.Round((currentCAC.Valor / currentMEP.Valor), 2);

                    var effectiveDateFrom = fromManual ? currentMEP.EffectiveDateFrom : dateNow.AddDays(1);
                    var effectiveDateTo = _holidaysService.GetNextWorkDayFromDate(effectiveDateFrom);

                    cacUsd.UploadDate = dateNow;

                    cacUsd.EffectiveDateFrom = new DateTime(effectiveDateFrom.Year, effectiveDateFrom.Month, effectiveDateFrom.Day, 0, 0, 0, 0);
                    cacUsd.EffectiveDateTo = new DateTime(effectiveDateTo.Year, effectiveDateTo.Month, effectiveDateTo.Day, 23, 59, 59, 999);
                    cacUsd.Description = $"Valor CAC USD - {cacUsd.RateType} - {effectiveDateFrom.ToString("dd/MM/yyyy")}";
                    cacUsd.Valor = CacUsdValue;
                    cacUsd.Source = EQuotationSource.COBRA;
                    cacUsd.UserId = "System";

                    //El corporate es igual al USD
                    cacUsdCorp.UploadDate = cacUsd.UploadDate;
                    cacUsdCorp.EffectiveDateFrom = cacUsd.EffectiveDateFrom;
                    cacUsdCorp.EffectiveDateTo = cacUsd.EffectiveDateTo;
                    cacUsdCorp.Description = $"Valor CAC USD - {cacUsdCorp.RateType} - {effectiveDateFrom.ToString("dd/MM/yyyy")}";
                    cacUsdCorp.Valor = cacUsd.Valor;
                    cacUsdCorp.Source = cacUsd.Source;
                    cacUsdCorp.UserId = cacUsd.UserId;

                    quotations.AddRange(new List<Quotation>() { cacUsd, cacUsdCorp });

                    var newQuotations = AddQuotations(quotations);
                    int minutesForCancellation = Int32.Parse(_configuration.GetSection("QuotationCancellationTimeInMinutes").Value);
                    var quotationIds = newQuotations.Select(x => x.Id).ToList();
                    BackgroundJob.Schedule(() => InformQuotations(quotationIds), TimeSpan.FromMinutes(minutesForCancellation));
                    if (newQuotations != null)
                    {
                        if (newQuotations.Where(x => x.RateType == cacUsd.RateType).Any())
                            Monitoreo.Monitor.Ok(MsgOkCacUsd, _servicios.CacUsd);
                        else
                            Monitoreo.Monitor.Critical(MsgCriticalCacUsd, _servicios.CacUsd);

                        if (newQuotations.Where(x => x.RateType == cacUsdCorp.RateType).Any())
                            Monitoreo.Monitor.Ok(MsgOkCacUsdCorp, _servicios.CacUsdCorp);
                        else
                            Monitoreo.Monitor.Critical(MsgCriticalCacUsdCorp, _servicios.CacUsdCorp);
                    }
                    else
                    {
                        Monitoreo.Monitor.Critical(MsgCriticalCacUsd, _servicios.CacUsd);
                        Monitoreo.Monitor.Critical(MsgCriticalCacUsdCorp, _servicios.CacUsdCorp);
                    }
                }
                else
                {
                    Serilog.Log.Error(@"No current CAC or MEP found, cannot calculate CAC USD. Quotations: MEP = {mep}, CAC = {cac}", currentMEP.Valor, currentCAC.Valor);
                    Monitoreo.Monitor.Critical(MsgCriticalCacUsd, _servicios.CacUsd);
                    Monitoreo.Monitor.Critical(MsgCriticalCacUsdCorp, _servicios.CacUsdCorp);
                }
            }
            catch (Exception ex)
            {
                Serilog.Log.Error(@"USDCAC Error loading quotation values from OLAP with items {@items}: {@err}", cacConfigs.Select(x => x.Item).ToList(), ex);
                Monitoreo.Monitor.Critical(MsgCriticalCacUsd, _servicios.CacUsd);
                Monitoreo.Monitor.Critical(MsgCriticalCacUsdCorp, _servicios.CacUsdCorp);
            }
        }

        public void GetUVA_UVAUSD()
        {
            string MsgOkUva = "Se obtuvo correctamente la cotización UVA";
            string MsgCriticalUva = "Ocurrió un error al intentar obtener cotización UVA";
            string MsgOkUvaUsd = "Se obtuvo correctamente la cotización UVA USD";
            string MsgCriticalUvaUsd = "Ocurrió un error al intentar obtener cotización UVA USD";
            bool OkUva = false;

            string localFilePath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "ExchangeRateFiles", "diar_uva.xls");
            string remoteUrl = _configuration.GetSection("UvaUrl").Value;

            if (File.Exists(localFilePath))
            {
                File.Delete(localFilePath);
            }

            AsyncHelper.RunSync(
                async () => await HttpHelper.DownloadFileAsync(remoteUrl, localFilePath));

            Serilog.Log.Information("Reading downloaded File..."); ;
            try
            {
                List<Quotation> quotations = new List<Quotation>();
                List<int> quotationsIds = new List<int>();

                var queryResult = GetUvaFromExcel(localFilePath);

                var quotationsByExcel = queryResult.Select(x => new UVA()
                {
                    Description = "UVA BCRA",
                    UserId = "Scrapper",
                    Source = EQuotationSource.BCRA,
                    UploadDate = LocalDateTime.GetDateTimeNow(),
                    EffectiveDateFrom = x.Date,
                    EffectiveDateTo = x.Date,
                    Valor = x.Quotation
                }).ToList();

                //Add Quotations UVA ARS
                var dateLastQuotationUva = GetLastQuotation("UVA");
                var totalQuotations = QuotationsByExcelFilter(quotationsByExcel, dateLastQuotationUva.EffectiveDateFrom);

                if (totalQuotations.Count > 0)
                {
                    quotations = AddQuotations(totalQuotations);
                }

                if (quotations.Count > 0)
                {
                    Monitoreo.Monitor.Ok(MsgOkUva, _servicios.Uva, _servicios.HostCobraTemp);
                    OkUva = true;
                    quotationsIds.AddRange(quotations.Select(x => x.Id).ToList());
                    int minutesForCancellation = Int32.Parse(_configuration.GetSection("QuotationCancellationTimeInMinutes").Value);
                    BackgroundJob.Schedule(() => InformQuotations(quotationsIds), TimeSpan.FromMinutes(minutesForCancellation));
                }
                else
                {
                    OkUva = true;
                    Monitoreo.Monitor.Ok("No se encontraron UVAs nuevos para cargar", _servicios.Uva, _servicios.HostCobraTemp);
                }

                //Add Quotations UVA USD
                var quotationUvaARSByDate = _exchangeRateFileRepository.GetQuotationByDate("UVA", LocalDateTime.GetDateTimeNow().AddDays(1).Date);
                var quotationDolarMep = GetCurrentQuotation("DolarMEP");

                if (quotationDolarMep.Valor > 0 && quotationUvaARSByDate.Valor > 0)
                {
                    var quotationUvaUSD = AddQuotation("UVAUSD", new UVAUSD()
                    {
                        Description = "UVA USD",
                        UserId = "Scrapper",
                        Source = EQuotationSource.BCRA,
                        UploadDate = LocalDateTime.GetDateTimeNow(),
                        EffectiveDateFrom = new DateTime(LocalDateTime.GetDateTimeNow().Year, LocalDateTime.GetDateTimeNow().Month, LocalDateTime.GetDateTimeNow().AddDays(1).Day, 0, 0, 0, 0),
                        EffectiveDateTo = new DateTime(LocalDateTime.GetDateTimeNow().Year, LocalDateTime.GetDateTimeNow().Month, LocalDateTime.GetDateTimeNow().AddDays(1).Day, 23, 59, 59, 999),
                        Valor = (double)Math.Round((quotationUvaARSByDate.Valor / quotationDolarMep.Valor), 2),
                    }, new User { Id = "Scrapper" }, EQuotationSource.COBRA);

                    if (quotationUvaUSD != null)
                    {
                        Monitoreo.Monitor.Ok(MsgOkUvaUsd, _servicios.UvaUsd);
                        var newQuotationCorp = GenerateQuotation(EQuotationSource.BCRA, RateTypes.Corporate, quotationUvaUSD.Valor, "UVAUSD");
                        var quotationUvaUSDCorp = AddQuotation("UVAUSDCORPORATE", newQuotationCorp, new User { Id = "Scrapper" }, EQuotationSource.COBRA);
                    }
                    else
                    {
                        Monitoreo.Monitor.Critical(MsgCriticalUvaUsd, _servicios.UvaUsd);
                        Log.Error("GetUVA_UVAUSD: Ocurrió un error al intentar obtener la cotización UvaUsd");
                    }

                }
                else
                {
                    //throw new NullReferenceException("No se encontró un Dolar MEP o un UVA ARS para calcular el UVA USD");
                }
            }
            catch (Exception ex)
            {
                Log.Error(ex, "Error processing file: {file}", localFilePath);
                if (!OkUva) Monitoreo.Monitor.Critical(MsgCriticalUva, _servicios.Uva, _servicios.HostCobraTemp);
                Monitoreo.Monitor.Critical(MsgCriticalUvaUsd, _servicios.UvaUsd);
            }
        }

        public QuotationExternal GetFromCbraHistorico(DateTime date)
        {

            var quotationBcra = new QuotationExternal();

            string MsgOkBCRA = "Se obtuvo correctamente la cotización BCRA";
            string MsgCriticalBCRA = "Ocurrió un error al intentar obtener cotización BCRA";

            string localFilePath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "ExchangeRateFiles", "serie_bcra.xls");
            string remoteUrl = _configuration.GetSection("BcraHistoricoUrl").Value;

            if (File.Exists(localFilePath))
            {
                File.Delete(localFilePath);
            }

            AsyncHelper.RunSync(
                async () => await HttpHelper.DownloadFileAsync(remoteUrl, localFilePath));

            Serilog.Log.Information("Reading downloaded File..."); ;
            try
            {
                List<QuotationExternal> quotations = new List<QuotationExternal>();
                List<int> quotationsIds = new List<int>();

                var queryResult = GetBcraFromExcel(localFilePath);

                if (!_holidaysService.IsAHoliday(date))
                {
                    var bcraByExcel = queryResult.Where(x => x.Date.Date == date.Date).FirstOrDefault();


                    if (bcraByExcel != null)
                    {
                        var effectiveDateFrom = bcraByExcel.Date.AddDays(1);
                        var effectiveDateTo = _holidaysService.GetNextWorkDayFromDate(effectiveDateFrom);

                        quotationBcra = new QuotationExternal
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
                            Valor = bcraByExcel.Valor
                        };

                        if (quotationBcra != null)
                        {
                            Monitoreo.Monitor.Ok(MsgOkBCRA, _servicios.Uva);
                        }
                        else
                        {
                            Monitoreo.Monitor.Critical(MsgCriticalBCRA, _servicios.UvaUsd);
                            Log.Error("GetUVA_UVAUSD: Ocurrió un error al intentar obtener la cotización UvaUsd");
                        }
                    }

                    return quotationBcra;
                }

                return quotationBcra;
            }
            catch (Exception ex)
            {
                Log.Error(ex, "GetFromCbraHistorico: Ocurrió un error al intentar generar una cotización Bcra");
            }

            return quotationBcra;
        }

        public dynamic GenerateQuotation(EQuotationSource source, string rateType, double valor, string className, DateTime? date = null)
        {
            try
            {
                var type = Assembly.GetExecutingAssembly().GetTypes().FirstOrDefault(x => x.IsClass && x.Name == className);
                if (type != null)
                {
                    var currentMEP = GetCurrentQuotation("DolarMEP");

                    dynamic quotation = Activator.CreateInstance(type);

                    var dateNow = date != null ? date : LocalDateTime.GetDateTimeNow();
                    var effectiveDateFrom = dateNow.Value.AddDays(1);
                    var effectiveDateTo = _holidaysService.GetNextWorkDayFromDate(effectiveDateFrom);

                    var contables = new List<string>()
                    {
                        RateTypes.BillBcuCompradorContable,
                        RateTypes.BillBcuVendedorContable,
                        RateTypes.BillBnaCompradorContable,
                        RateTypes.BillBnaVendedorContable,
                        RateTypes.DivBnaCompradorContable,
                        RateTypes.DivBnaVendedorContable
                    };

                    if (rateType == RateTypes.Cac) // CAC
                    {
                        quotation.EffectiveDateFrom = new DateTime(dateNow.Value.Year, dateNow.Value.Month, dateNow.Value.Day, 0, 0, 0, 000);
                        quotation.EffectiveDateTo = new DateTime(
                                                quotation.EffectiveDateFrom.AddMonths(1).Year, //year of the date
                                                quotation.EffectiveDateFrom.AddMonths(1).Month, //month of the date
                                                DateTime.DaysInMonth(quotation.EffectiveDateFrom.AddMonths(1).Year, quotation.EffectiveDateFrom.AddMonths(1).Month), //last day of the resultant month
                                                23, 59, 59, 999); //last second of the day;
                    }
                    else if (contables.Contains(rateType)) // Contables
                    {
                        quotation.EffectiveDateFrom = new DateTime(dateNow.Value.Year, dateNow.Value.Month, dateNow.Value.Day, 0, 0, 0, 0);
                        quotation.EffectiveDateTo = new DateTime(dateNow.Value.Year, dateNow.Value.Month, dateNow.Value.Day, 23, 59, 59, 999);
                    }
                    else // Other quotations
                    {
                        quotation.EffectiveDateFrom = new DateTime(effectiveDateFrom.Year, effectiveDateFrom.Month, effectiveDateFrom.Day, 0, 0, 0, 0);
                        quotation.EffectiveDateTo = new DateTime(effectiveDateTo.Year, effectiveDateTo.Month, effectiveDateTo.Day, 23, 59, 59, 999);
                    }

                    quotation.Description = $"Valor {quotation.FromCurrency}_{quotation.ToCurrency} {rateType} - {effectiveDateTo:dd/MM/yyyy}";
                    quotation.RateType = rateType;
                    quotation.Source = source;
                    quotation.Valor = Math.Round((double)(rateType == RateTypes.Cryptos && quotation.ToCurrency == "ARS" ? valor * currentMEP.Valor : valor), 2);
                    return quotation;
                }
            }
            catch (Exception ex)
            {
                Log.Error(ex, "GenerateQuotation: Ocurrió un error al intentar generar una cotización {rateType}", rateType);
            }
            return null;
        }

        public List<dynamic> GenerateQuotations(List<QuotationExternal> quotations, EQuotationSource source)
        {
            var newQuotations = new List<dynamic>();
            try
            {
                switch (source)
                {

                    case EQuotationSource.BCU:
                        // Add newQuotations
                        foreach (var quotation in quotations)
                        {
                            var newQuotation = GenerateQuotation(quotation.Source, quotation.RateType, quotation.Valor, quotation.FromCurrency + quotation.ToCurrency);
                            if (newQuotation != null)
                                newQuotations.Add(newQuotation);
                        }

                        // Add Contables
                        foreach (var quotation in quotations)
                        {
                            var rateType = quotation.RateType == RateTypes.BillBcuComprador ? RateTypes.BillBcuCompradorContable :
                                           quotation.RateType == RateTypes.BillBcuVendedor ? RateTypes.BillBcuVendedorContable : String.Empty;
                            var newQuotation = GenerateQuotation(quotation.Source, rateType, quotation.Valor, quotation.FromCurrency + quotation.ToCurrency);
                            if (newQuotation != null)
                                newQuotations.Add(newQuotation);
                        }

                        // Add Corporate
                        foreach (var quotation in quotations)
                        {
                            if (quotation.RateType == RateTypes.BillBcuComprador && quotation.FromCurrency == "USD")
                            {
                                var newQuotation = GenerateQuotation(quotation.Source, RateTypes.Corporate, quotation.Valor, quotation.FromCurrency + quotation.ToCurrency);
                                if (newQuotation != null)
                                    newQuotations.Add(newQuotation);
                            }
                        }

                        break;

                    case EQuotationSource.BNA:
                        // Add newQuotations
                        foreach (var quotation in quotations)
                        {
                            var newQuotation = GenerateQuotation(quotation.Source, quotation.RateType, quotation.Valor, quotation.FromCurrency);
                            if (newQuotation != null)
                                newQuotations.Add(newQuotation);
                        }

                        // Add Contables div USD
                        foreach (var quotation in quotations)
                        {
                            if ((quotation.RateType == RateTypes.DivBnaComprador || quotation.RateType == RateTypes.DivBnaVendedor) && quotation.FromCurrency == "USD")
                            {
                                var rateType = quotation.RateType == RateTypes.DivBnaComprador ? RateTypes.DivBnaCompradorContable :
                                               quotation.RateType == RateTypes.DivBnaVendedor ? RateTypes.DivBnaVendedorContable : String.Empty;
                                var newQuotationContable = GenerateQuotation(quotation.Source, rateType, quotation.Valor, quotation.FromCurrency);
                                if (newQuotationContable != null)
                                    newQuotations.Add(newQuotationContable);
                            }
                        }

                        foreach (var quotation in quotations)
                        {
                            // Add EURUSDs
                            var USD = quotations.FirstOrDefault(x => x.RateType == quotation.RateType && x.FromCurrency == "USD");
                            if (quotation.FromCurrency == "EUR" && USD != null)
                            {
                                var newQuotationEURUSD = GenerateQuotation(quotation.Source, quotation.RateType, Math.Round(quotation.Valor / USD.Valor, 2), "EURUSD");
                                if (newQuotationEURUSD != null)
                                {
                                    newQuotations.Add(newQuotationEURUSD);

                                    // Add Corporate
                                    if (newQuotationEURUSD.RateType == RateTypes.BillBnaComprador)
                                    {
                                        var newQuotationCorporate = GenerateQuotation(quotation.Source, RateTypes.Corporate, newQuotationEURUSD.Valor, "EURUSD");
                                        if (newQuotationCorporate != null)
                                            newQuotations.Add(newQuotationCorporate);
                                    }
                                }
                            }
                        }
                        break;
                    case EQuotationSource.COINAPI:

                        var tempQuotations = new List<dynamic>();

                        // Add new quotations
                        foreach (var quotation in quotations)
                        {
                            var newQuotation = GenerateQuotation(quotation.Source, RateTypes.Cryptos, quotation.Valor, quotation.FromCurrency);
                            if (newQuotation != null && newQuotation.Valor != 0)
                                tempQuotations.Add(newQuotation);
                        }

                        // Add to ARS
                        foreach (var quotation in quotations)
                        {
                            var newQuotation = GenerateQuotation(quotation.Source, RateTypes.Cryptos, quotation.Valor, quotation.FromCurrency + "ARS");
                            if (newQuotation != null && newQuotation.Valor != 0)
                                tempQuotations.Add(newQuotation);
                        }

                        // Add USDT
                        var newUSDT = GenerateQuotation(EQuotationSource.COINAPI, RateTypes.Cryptos, 1, "USDTARS");
                        if (newUSDT != null && newUSDT.Valor != 0)
                            tempQuotations.Add(newUSDT);

                        // Add Corporates
                        foreach (var quotation in tempQuotations)
                        {
                            var className = quotation.ToCurrency == "USD" ? quotation.FromCurrency : quotation.FromCurrency + quotation.ToCurrency;
                            var newCorporate = GenerateQuotation(quotation.Source, RateTypes.Corporate, quotation.Valor, className);
                            if (newCorporate != null && newCorporate.Valor != 0)
                                newQuotations.Add(newCorporate);
                        }

                        newQuotations.AddRange(tempQuotations);

                        break;
                }

                return newQuotations;
            }
            catch (Exception ex)
            {
                Log.Error(ex, "Error generating quotations");
            }

            return null;
        }

        public List<dynamic> GenerateQuotations(List<QuotationExternal> quotations, DateTime? date = null)
        {
            dynamic newQuotation = null;
            var newQuotations = new List<dynamic>();
            try
            {
                foreach (var quotation in quotations)
                {
                    var dateBefore = quotation.EffectiveDateFrom.AddDays(-1);
                    switch (quotation.Source)
                    {
                        case EQuotationSource.INVERTIRONLINE:

                            var dolarMep = new DolarMEP
                            {
                                Description = quotation.Description,
                                EffectiveDateFrom = quotation.EffectiveDateFrom,
                                EffectiveDateTo = quotation.EffectiveDateTo,
                                FromCurrency = quotation.FromCurrency,
                                ToCurrency = quotation.ToCurrency,
                                RateType = RateTypes.UsdMEP,
                                Source = quotation.Source,
                                UploadDate = quotation.UploadDate,
                                UserId = quotation.UserId,
                                Valor = quotation.Valor,
                                Especie = quotation.Especie,
                                Id = quotation.Id
                            };
                            newQuotations.Add(dolarMep);
                            break;

                        case EQuotationSource.CAMARCO:
                            var webData = quotation.Description[(quotation.Description.IndexOf('-') + 2)..];
                            if (!CheckCacExists(webData))
                            {
                                newQuotation = GenerateQuotation(quotation.Source, quotation.RateType, quotation.Valor, quotation.FromCurrency);
                                newQuotation.Description = quotation.Description;
                                newQuotations.Add(newQuotation);
                            }
                            break;

                        case EQuotationSource.BCRA:
                            newQuotation = GenerateQuotation(quotation.Source, quotation.RateType, quotation.Valor, quotation.FromCurrency, date != null ? date : dateBefore);
                            newQuotation.Description = quotation.Description;
                            newQuotations.Add(newQuotation);
                            break;

                        case EQuotationSource.BCU:
                            // Add newQuotations

                            newQuotation = GenerateQuotation(quotation.Source, quotation.RateType, quotation.Valor, quotation.FromCurrency + quotation.ToCurrency, date != null ? date : dateBefore);
                            if (newQuotation != null)
                                newQuotations.Add(newQuotation);

                            // Add Contables

                            var rateType = quotation.RateType == RateTypes.BillBcuComprador ? RateTypes.BillBcuCompradorContable :
                                           quotation.RateType == RateTypes.BillBcuVendedor ? RateTypes.BillBcuVendedorContable : String.Empty;
                            newQuotation = GenerateQuotation(quotation.Source, rateType, quotation.Valor, quotation.FromCurrency + quotation.ToCurrency, date != null ? date : dateBefore);
                            if (newQuotation != null)
                                newQuotations.Add(newQuotation);

                            // Add Corporate

                            if (quotation.RateType == RateTypes.BillBcuComprador && quotation.FromCurrency == "USD")
                            {
                                newQuotation = GenerateQuotation(quotation.Source, RateTypes.Corporate, quotation.Valor, quotation.FromCurrency + quotation.ToCurrency, date != null ? date : dateBefore);
                                if (newQuotation != null)
                                    newQuotations.Add(newQuotation);
                            }

                            break;

                        case EQuotationSource.BNA:
                            // Add newQuotations

                            newQuotation = GenerateQuotation(quotation.Source, quotation.RateType, quotation.Valor, quotation.FromCurrency, date != null ? date : dateBefore);
                            if (newQuotation != null)
                                newQuotations.Add(newQuotation);

                            // Add Contables div USD

                            if ((quotation.RateType == RateTypes.DivBnaComprador || quotation.RateType == RateTypes.DivBnaVendedor) && quotation.FromCurrency == "USD")
                            {
                                rateType = quotation.RateType == RateTypes.DivBnaComprador ? RateTypes.DivBnaCompradorContable :
                                               quotation.RateType == RateTypes.DivBnaVendedor ? RateTypes.DivBnaVendedorContable : String.Empty;
                                var newQuotationContable = GenerateQuotation(quotation.Source, rateType, quotation.Valor, quotation.FromCurrency, date != null ? date : dateBefore);
                                if (newQuotationContable != null)
                                    newQuotations.Add(newQuotationContable);
                            }

                            // Add EURUSDs
                            var USD = quotations.FirstOrDefault(x => x.RateType == quotation.RateType && x.FromCurrency == "USD");
                            if (quotation.FromCurrency == "EUR" && USD != null)
                            {
                                var newQuotationEURUSD = GenerateQuotation(quotation.Source, quotation.RateType, Math.Round(quotation.Valor / USD.Valor, 2), "EURUSD", date != null ? date : dateBefore);
                                if (newQuotationEURUSD != null)
                                {
                                    newQuotations.Add(newQuotationEURUSD);

                                    // Add Corporate
                                    if (newQuotationEURUSD.RateType == RateTypes.BillBnaComprador)
                                    {
                                        var newQuotationCorporate = GenerateQuotation(quotation.Source, RateTypes.Corporate, newQuotationEURUSD.Valor, "EURUSD", date != null ? date : dateBefore);
                                        if (newQuotationCorporate != null)
                                            newQuotations.Add(newQuotationCorporate);
                                    }
                                }
                            }

                            break;
                        case EQuotationSource.COINAPI:

                            var tempQuotations = new List<dynamic>();

                            // Add new quotations

                            newQuotation = GenerateQuotation(quotation.Source, RateTypes.Cryptos, quotation.Valor, quotation.FromCurrency, date != null ? date : dateBefore);
                            if (newQuotation != null && newQuotation.Valor != 0)
                                tempQuotations.Add(newQuotation);


                            // Add to ARS

                            newQuotation = GenerateQuotation(quotation.Source, RateTypes.Cryptos, quotation.Valor, quotation.FromCurrency + "ARS", date != null ? date : dateBefore);
                            if (newQuotation != null && newQuotation.Valor != 0)
                                tempQuotations.Add(newQuotation);


                            // Add USDT
                            var newUSDT = GenerateQuotation(EQuotationSource.COINAPI, RateTypes.Cryptos, 1, "USDTARS", date != null ? date : dateBefore);
                            if (newUSDT != null && newUSDT.Valor != 0)
                                tempQuotations.Add(newUSDT);

                            // Add Corporates

                            var className = quotation.ToCurrency == "USD" ? quotation.FromCurrency : quotation.FromCurrency + quotation.ToCurrency;
                            var newCorporate = GenerateQuotation(quotation.Source, RateTypes.Corporate, quotation.Valor, className, date != null ? date : dateBefore);
                            if (newCorporate != null && newCorporate.Valor != 0)
                                newQuotations.Add(newCorporate);


                            newQuotations.AddRange(tempQuotations);

                            break;
                    }
                }


                return newQuotations;
            }
            catch (Exception ex)
            {
                Log.Error(ex, "Error generating quotations");
            }

            return null;
        }


        private List<UvaExchageRateFile> GetUvaFromExcel(string path)
        {
            var excelMapper = new ExcelMapper(path)
            {
                HeaderRowNumber = 25,
                MinRowNumber = 27
            };
            excelMapper.AddMapping<UvaExchageRateFile>("fecha", p => p.Date)
                .SetCellUsing((c, o) =>
                {
                    if (o is null) c.SetCellValue("01/01/0001");
                    else c.SetCellValue((DateTime)o);
                })
                .SetPropertyUsing(v =>
                    (v as string) == "NULL" ? null : Convert.ChangeType(v, typeof(DateTime), new CultureInfo("es-AR")));

            excelMapper.AddMapping<UvaExchageRateFile>("uva1", p => p.Quotation)
                .SetCellUsing((c, o) =>
                {
                    if (o is null) c.SetCellValue(0);
                    else c.SetCellValue((double)o);
                })
                .SetPropertyUsing(v =>
                    (v as string) == "NULL" ? null : Convert.ChangeType(v, typeof(double), new CultureInfo("es-AR")));

            return excelMapper.Fetch<UvaExchageRateFile>().ToList();
        }

        private List<BcraExchageRateFile> GetBcraFromExcel(string filePath)
        {
            var dataList = new List<BcraExchageRateFile>();
            using (var stream = File.Open(filePath, FileMode.Open, FileAccess.Read))
            {
                using (var reader = ExcelReaderFactory.CreateReader(stream))
                {
                    // Avanzar hasta la hoja deseada
                    reader.Read(); // Hoja 1

                    // Saltar las primeras 4 filas
                    for (int i = 0; i < 3; i++)
                    {
                        reader.Read();
                    }

                    while (reader.Read())
                    {
                        if (!reader.IsDBNull(2) && !reader.IsDBNull(3))
                        {
                            var data = new BcraExchageRateFile();

                            data.Date = reader.GetDateTime(2);
                            data.Valor = reader.GetDouble(3);

                            dataList.Add(data);
                        }
                    }
                }
            }

            return dataList;
        }


        private List<Quotation> QuotationsByExcelFilter(List<UVA> quotationsUva, DateTime dateLastQuotationUva)
        {
            return quotationsUva.Where(x => x.EffectiveDateFrom.Date > dateLastQuotationUva.Date).Select(x => new UVA()
            {
                Description = x.Description,
                UserId = x.UserId,
                Source = x.Source,
                UploadDate = x.UploadDate,
                EffectiveDateFrom = new DateTime(x.EffectiveDateFrom.Year, x.EffectiveDateFrom.Month, x.EffectiveDateFrom.Day, 0, 0, 0, 0),
                EffectiveDateTo = new DateTime(x.EffectiveDateTo.Year, x.EffectiveDateTo.Month, x.EffectiveDateTo.Day, 23, 59, 59, 999),
                Valor = x.Valor
            }).Cast<Quotation>().ToList();
        }

        private void ReportToOracleBySGF(List<QuotationOracleViewModel> dataForOracle)
        {
            try
            {
                _restClient.BaseUrl = new Uri(_apiServicesConfig.Get(ApiServicesConfig.SgfApi).Url);
                RestRequest request = new RestRequest("/Cotizacion/GenerarArchivoCotizacion", Method.POST)
                {
                    RequestFormat = DataFormat.Json
                };
                request.AddHeader("token", _apiServicesConfig.Get(ApiServicesConfig.SgfApi).Token);
                request.AddJsonBody(dataForOracle);

                IRestResponse response = Task.Run(async () => await _restClient.ExecuteAsync(request)).ConfigureAwait(false).GetAwaiter().GetResult();

                if (response.IsSuccessful)
                {
                    Log.Information("InformNewQuotation a {url} de las cotizaciones: {@oracleQuotations}", request.Resource, dataForOracle);
                    Log.Debug("InformNewQuotation to SGF \n Cotizaciones: {@oracleQuotations} \n request: {@request} \n response:{@response}", dataForOracle, request, response);
                }
                else
                {
                    Log.Error("InformNewQuotation a {url} de las cotizaciones: {@oracleQuotations}. Response: {@response}", request.Resource, dataForOracle, response);
                }
            }
            catch (Exception ex)
            {
                Log.Error("An Error was found in ExchangeRateService > InformQuotations > ReportToOracleBySGF > Error", ex.Message);

            }

        }

        private void ReportToSGC(List<QuotationOracleViewModel> dataForSGC)
        {
            try
            {
                _restClient.BaseUrl = new Uri(_apiServicesConfig.Get(ApiServicesConfig.SgcApi).Url);
                RestRequest requestSGC = new RestRequest("/Cotizacion/Nueva", Method.POST)
                {
                    RequestFormat = DataFormat.Json
                };
                requestSGC.AddHeader("token", _apiServicesConfig.Get(ApiServicesConfig.SgcApi).Token);

                requestSGC.AddJsonBody(dataForSGC);

                IRestResponse responseSGC = Task.Run(async () => await _restClient.ExecuteAsync(requestSGC)).ConfigureAwait(false).GetAwaiter().GetResult();
                if (responseSGC.IsSuccessful)
                {
                    Log.Information("InformNewQuotation a {url} de las cotizaciones: {@oracleQuotations}", requestSGC.Resource, dataForSGC);
                    Log.Debug("InformNewQuotation to SGC \n Cotizaciones: {@oracleQuotations} \n request: {@request} \n response:{@response}", dataForSGC, requestSGC, responseSGC);
                }
                else
                {
                    Log.Error("Error en InformNewQuotation a {url} de las cotizaciones: {@oracleQuotations}. Response: {@responseSGC}", requestSGC.Resource, dataForSGC, responseSGC);
                }
            }
            catch (Exception ex)
            {
                Log.Error("An Error was found in ExchangeRateService > InformQuotations > ReportBySGC> Error", ex.Message);

            }

        }
    }

    public class UvaExchageRateFile
    {
        public DateTime Date { get; set; }
        public double Quotation { get; set; }
    }

    public class BcraExchageRateFile
    {
        public DateTime Date { get; set; }
        public double Valor { get; set; }
    }
}
