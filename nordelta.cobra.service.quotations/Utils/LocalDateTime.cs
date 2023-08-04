using TimeZoneConverter;

namespace nordelta.cobra.service.quotations.Utils
{
    public static class LocalDateTime
    {
        public static DateTime GetDateTimeNow()
        {
            TimeZoneInfo argentinaTimeZoneInfo = TZConvert.GetTimeZoneInfo("Argentina Standard Time");
            return TimeZoneInfo.ConvertTime(DateTime.Now, TimeZoneInfo.Local, argentinaTimeZoneInfo);
        }

        public static TimeZoneInfo GetTimeZone(string timeZoneId)
        {
            TimeZoneInfo timeZoneInfo = TZConvert.GetTimeZoneInfo(timeZoneId);
            return timeZoneInfo;
        }
    }
}
