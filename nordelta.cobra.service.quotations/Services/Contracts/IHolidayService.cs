namespace nordelta.cobra.service.quotations.Services.Contracts
{
    public interface IHolidayService
    {
        public Task<DateTime> GetNextWorkDayFromDateAsync(DateTime effDate);
        public Task<List<HolidayDay>> GetHolidaysAsync();
        public Task<bool> IsAHolidayAsync(DateTime date, List<HolidayDay>? holidays);
    }
}
