using nordelta.cobra.webapi.Models;
using System.Collections.Generic;

namespace nordelta.cobra.webapi.Services.Contracts
{
    public interface IFinanceQuotationsService
    {
        void UpdateQuotations(IEnumerable<FinanceQuotation> quotes);
        QuotationPackage GetQuotations();
        void SaveQuotations();
    }
}