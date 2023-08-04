using nordelta.cobra.webapi.Models;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace nordelta.cobra.webapi.Repositories.Contracts
{
    public interface IHolidaysRepository
    {
        bool HasHolidays();
        void SaveHolidaysAsync(IEnumerable<HolidayDay> holidays);
        IEnumerable<HolidayDay> GetHolidaysByYear(int year);
    }
}
