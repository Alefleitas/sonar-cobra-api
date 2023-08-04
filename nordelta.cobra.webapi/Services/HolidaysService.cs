using Microsoft.Extensions.Options;
using nordelta.cobra.webapi.Configuration;
using nordelta.cobra.webapi.Models;
using nordelta.cobra.webapi.Repositories.Contracts;
using nordelta.cobra.webapi.Services.Contracts;
using RestSharp;
using Serilog;
using System;
using System.Collections.Generic;
using System.Linq;
using Hangfire;
using nordelta.cobra.webapi.Utils;

namespace nordelta.cobra.webapi.Services
{
    public class HolidaysService: IHolidaysService
    {
        protected readonly IHolidaysRepository _holidaysRepository;
        private readonly IRestClient _restClient;
        private readonly IOptionsMonitor<ApiServicesConfig> _holidaysApiConfig;

        public HolidaysService(IHolidaysRepository holidaysRepository, IRestClient restClient, IOptionsMonitor<ApiServicesConfig> options)
        {
            ApiServicesConfig apiServicesConfig = options.Get(ApiServicesConfig.HolidaysApi);
            restClient.BaseUrl = new Uri(apiServicesConfig.Url);
            _restClient = restClient;
            _holidaysRepository = holidaysRepository;
            _holidaysApiConfig = options;
        }

        [DisableConcurrentExecution(timeoutInSeconds: 1800)]
        public void SyncHolidays()
        {
            var feriados = new List<HolidayDay>();
            List<int> años = new List<int> { LocalDateTime.GetDateTimeNow().Year, LocalDateTime.GetDateTimeNow().Year - 1 };
            
            foreach(var año in años)
            {
                try {
                    Log.Debug(@"Starting Holidays Syncing for year {0}...", año);
                    RestRequest request = new RestRequest($"/{año}", Method.GET);

                    // execute the request
                    IRestResponse<dynamic> holidaysResponse = (RestResponse<dynamic>)_restClient.Execute<dynamic>(request);

                    if (holidaysResponse.IsSuccessful && holidaysResponse.Data != null)
                    {
                        foreach(var obj in holidaysResponse.Data)
                        {

                            if (obj["motivo"].ToString().ToLower().Contains("viernes santo"))
                            {
                                if (obj["motivo"].ToString().ToLower().Contains("viernes santo"))
                                {
                                    feriados.Add(new HolidayDay
                                    {
                                        Dia = unchecked((int)obj["dia"]) - 1,
                                        Mes = unchecked((int)obj["mes"]),
                                        Motivo = obj["motivo"],
                                        Anio = año
                                    });
                                }
                            }

                            feriados.Add(new HolidayDay {
                               Dia = unchecked((int)obj["dia"]),
                               Mes = unchecked((int)obj["mes"]),
                               Motivo = obj["motivo"],
                               Anio = año
                            });


                        }

                    }
                } catch(Exception ex) {
                    Log.Error("Error sincronizando feriados. Exception detail: {@ex}", ex);
                }
            }
            
            try
            {
                _holidaysRepository.SaveHolidaysAsync(feriados);
            }
            catch(Exception ex) 
            {
                Log.Error("Error guardando feriados. Exception detail: {@ex}", ex);
            }

}

        public DateTime GetNextWorkDayFromDate(DateTime date)
        {
            if (!_holidaysRepository.HasHolidays())
            {
                SyncHolidays();
            }
            while (IsAHoliday(date) || IsWeekend(date))
                date = date.AddDays(1);
            return date;
        }
        public DateTime GetPreviousWorkDayFromDate(DateTime date)
        {
            if (!_holidaysRepository.HasHolidays())
            {
                SyncHolidays();
            }
            while (IsAHoliday(date) || IsWeekend(date))
                date = date.AddDays(-1);
            return date;
        }

        public bool IsAHoliday(DateTime date)
        {
            if (!_holidaysRepository.HasHolidays())
            {
                SyncHolidays();
            }
            var holidayYear = _holidaysRepository.GetHolidaysByYear(date.Year);
            return holidayYear.Any(x => x.Dia == date.Day && x.Mes == date.Month);
        }

        public bool IsWeekend(DateTime date)
        {
            if (!_holidaysRepository.HasHolidays())
            {
                SyncHolidays();
            }
            return (date.DayOfWeek == DayOfWeek.Sunday || date.DayOfWeek == DayOfWeek.Saturday);
        }
    }
}
