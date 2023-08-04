using nordelta.cobra.webapi.Models;

namespace nordelta.cobra.webapi.Controllers.Contracts
{
    public interface IFilterableByDepartment
    {
        AccountBalance.EDepartment GetDepartment();
        string GetPublishDebt();
    }
}