using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Threading.Tasks;

namespace nordelta.cobra.webapi.Extensions
{
    public static class DateTimeExtensions
    {
        public static int DayDiff(this DateTime d1, DateTime d2)
        {
            return (int)Math.Ceiling((d1 - d2).TotalDays);
        }
        public static int WeekDiff(this DateTime d1, DateTime d2, DayOfWeek startOfWeek = DayOfWeek.Monday)
        {
            var diff = d2.Subtract(d1);

            var weeks = (int)diff.Days / 7;

            var remainingDays = diff.Days % 7;
            var cal = CultureInfo.InvariantCulture.Calendar;
            var d1WeekNo = cal.GetWeekOfYear(d1, CalendarWeekRule.FirstFullWeek, startOfWeek);
            var d1PlusRemainingWeekNo = cal.GetWeekOfYear(d1.AddDays(remainingDays), CalendarWeekRule.FirstFullWeek, startOfWeek);

            if (d1WeekNo != d1PlusRemainingWeekNo)
                weeks++;

            return weeks;
        }
        public static int MonthDiff(this DateTime d1, DateTime d2)
        {
            int m1;
            int m2;
            if (d1 < d2)
            {
                m1 = (d2.Month - d1.Month);//for years
                m2 = (d2.Year - d1.Year) * 12; //for months
            }
            else
            {
                m1 = (d1.Month - d2.Month);//for years
                m2 = (d1.Year - d2.Year) * 12; //for months
            }

            return m1 + m2;
        }
    }
}
