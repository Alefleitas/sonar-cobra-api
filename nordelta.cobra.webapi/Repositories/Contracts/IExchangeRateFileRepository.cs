using nordelta.cobra.webapi.Controllers.ViewModels;
using nordelta.cobra.webapi.Models;
using System;
using System.Collections.Generic;

namespace nordelta.cobra.webapi.Repositories.Contracts
{
    public interface IExchangeRateFileRepository
    {
        IUnitOfWork UnitOfWork { get; }
        void Add(ExchangeRateFile exchangeRateFile);
        void AddBonoConfig(Bono bono);
        Bono GetLastBonosConfig();
        bool CheckDolarMepJobWasExecuted();
        bool CheckQuotationExists(Quotation quotation);
        ExchangeRateFile GetByFileName(string fileName);
        ExchangeRateFile GetLastExchangeRateFileAvailable();
        List<QuotationViewModel> GetQuotationTypes();
        dynamic GetLastQuotation(string type);
        dynamic GetCurrentQuotation(string type, bool lastQuote = false);
        dynamic AddQuotation(string quotationType, dynamic quotation, User user, EQuotationSource loadType);
        List<Quotation> AddQuotations(List<Quotation> quotations);
        Quotation GetQuotationById(int quotationId);
        List<Quotation> GetQuotationsByIds(List<int> quotationIds);
        bool CheckCacExists(string cacDate);
        void CancelQuotation(Quotation quotation);
        dynamic GetQuotationByDate(string type, DateTime date);
        List<object> GetAllQuotations(string type);
        List<Quotation> GetAllQuotationsToday();
        dynamic GetQuotationBetweenDate(string type, DateTime date);
    }
}
