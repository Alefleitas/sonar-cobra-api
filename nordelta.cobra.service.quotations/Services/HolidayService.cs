using Microsoft.Extensions.Options;
using nordelta.cobra.service.quotations.Configuration;
using nordelta.cobra.service.quotations.Jobs;
using nordelta.cobra.service.quotations.Services.Contracts;
using nordelta.cobra.service.quotations.Utils;
using ServiceStack;

namespace nordelta.cobra.service.quotations.Services
{
    public  class HolidayService: IHolidayService
    {
        private readonly ILogger<HolidayService> _logger;
        private readonly IOptions<HolidayApiConfiguration> _holidayApi;

        public HolidayService(
            ILogger<HolidayService> logger,
            IOptions<HolidayApiConfiguration> holidayApi) =>
            (_logger, _holidayApi) = (logger, holidayApi);
       
        public async Task<List<HolidayDay>> GetHolidaysAsync()
        {
            var holidays = new List<HolidayDay>();
            const string route = "feriados/{0}";
            int currentYear = LocalDateTime.GetDateTimeNow().Year;

            try
            {
                _logger.LogDebug($"Starting Holidays Syncing for year {currentYear}...");

                var url = _holidayApi.Value.Url
                    .AppendUrlPathsRaw(route.Fmt(currentYear));

                var quotationRequest = await url
                    .GetJsonFromUrlAsync();
                var holidaysResponse = quotationRequest.FromJson<List<HolidayResponse>>();


                if (holidaysResponse is null) return holidays;

                holidays.AddRange(holidaysResponse.Select(obj =>
                    new HolidayDay
                    {
                        Day = int.Parse(obj.dia),
                        Month = int.Parse(obj.mes),
                        Reason = obj.motivo,
                        Year = currentYear
                    }));

                holidays.ForEach(obj =>
                {
                    if (obj.Reason.ToLower().Contains("viernes santo"))
                    {
                        holidays.Add(new HolidayDay
                        {
                            Day = (obj.Day - 1),
                            Month = obj.Month,
                            Id = obj.Id,
                            Year = currentYear,
                            Reason = obj.Reason
                        });
                    }
                });

                return holidays;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error in GetHolidays");
                return holidays;
            }
        }

        public async Task<bool> IsAHolidayAsync(DateTime date, List<HolidayDay>? holidays)
        {
            holidays ??= await GetHolidaysAsync();

            return holidays.Any(x => x.Day == date.Day && x.Month == date.Month);
        }

        public async Task<DateTime> GetNextWorkDayFromDateAsync(DateTime effDate)
        {
            var holidays = await GetHolidaysAsync();

            static bool IsWeekend(DateTime date)
            {
                return date.DayOfWeek is DayOfWeek.Sunday or DayOfWeek.Saturday;
            }

            while (await IsAHolidayAsync(effDate, holidays) || IsWeekend(effDate))
                effDate = effDate.AddDays(1);
            return effDate;
        }
    }
    public class HolidayResponse
    {
        public string dia { get; set; }
        public string mes { get; set; }
        public string motivo { get; set; }

    }
    public class HolidayDay
    {
        public int Id { get; set; }
        public int Day { get; set; }
        public int Month { get; set; }
        public string Reason { get; set; }
        public int Year { get; set; }
    }

}
