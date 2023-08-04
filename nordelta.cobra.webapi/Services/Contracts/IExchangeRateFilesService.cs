using nordelta.cobra.webapi.Controllers.ViewModels;
using nordelta.cobra.webapi.Models;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace nordelta.cobra.webapi.Services.Contracts
{
    public interface IExchangeRateFilesService
    {
        void ProcessAllFiles();
        ExchangeRateFile GetLastExchangeRateFile();
        void PublishQuotationByBono(IEnumerable<Bono> bonos);
        List<Bono> GetLastBonosConfig();
        List<QuotationViewModel> GetQuotationTypes();
        List<string> GetSourceTypes();
        dynamic GetLastQuotation(string type);
        dynamic GetCurrentQuotation(string type, bool lastQuote = false);
        dynamic AddQuotation(string quotationType, dynamic quotation, User user, EQuotationSource loadType);
        dynamic AddQuotationsAndInform(dynamic data);
        bool CancelQuotation(int quotationId);
        void GetUSDCAC(bool fromManual);
        void GetUVA_UVAUSD();
        double GetLastUsdFromDetalleDeuda();
        bool CheckDolarMepJobWasExecuted();
        void InformQuotations(List<int> quotationIds);
        void InformQuotations(List<dynamic> quotations, List<string> systemsToInform);
        List<dynamic> GetAllQuotations(string type);
        List<dynamic> AddQuotations(dynamic quotations, User user);
        bool CheckCacExists(string webDate);
        dynamic GenerateQuotation(EQuotationSource source, string rateType, double valor, string className, DateTime? date = null);
        List<dynamic> GenerateQuotations(List<QuotationExternal> quotations, EQuotationSource source);
        List<dynamic> GenerateQuotations(List<QuotationExternal> quotations, DateTime? date = null);
        List<dynamic> AddCryptocurrencyQuotations(dynamic quotations, User user);
        bool ExecuteGetDolarMepAsync();
        List<QuotationExternal> CheckQuotationsExists(List<QuotationExternal> quotations, DateTime date);
        Task<List<QuotationExternal>> GetSourceQuotationsAsync(DateTime date, IEnumerable<string> quotes);
        Task<List<BonoConfig>> GetEspeciesAsync();
    }
}
