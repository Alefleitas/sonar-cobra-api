using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TimeZoneConverter;

namespace cobra.service.mail.listener.communications.Utils
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
