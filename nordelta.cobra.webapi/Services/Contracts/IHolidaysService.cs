using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using nordelta.cobra.webapi.Models;

namespace nordelta.cobra.webapi.Services.Contracts
{
    public interface IHolidaysService
    {
        void SyncHolidays();
        DateTime GetNextWorkDayFromDate(DateTime date);
        DateTime GetPreviousWorkDayFromDate(DateTime date);
        bool IsAHoliday(DateTime date);
        bool IsWeekend(DateTime date);
    }
}
