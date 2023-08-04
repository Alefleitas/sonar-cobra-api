using nordelta.cobra.webapi.Models;
using System;
using System.Collections.Generic;

namespace nordelta.cobra.webapi.Repositories.Contracts
{
    public interface IHistoricQuotationsRepository
    {
        List<HistoricQuotations> GetAllHistoricQuotations();
        List<HistoricQuotations> GetHistoricQuotations(ETipoQuote tipo, string titulo);
        List<HistoricQuotations> GetHistoricQuotationsBySubtipo(ESubtipoQuote subtipo);
        HistoricQuotations GetMostRecentHistoricQuotation(string titulo);
        HistoricQuotations GetMostRecentHistoricQuotation(ETipoQuote tipo, ESubtipoQuote subtipo);
        List<HistoricQuotations> GetHistoricQuotationsByDate(DateTime date);
        void SaveQuotations(IEnumerable<FinanceQuotation> quotes);
        bool HasHistoricQuotations();
    }
}
