using nordelta.cobra.service.quotations.Jobs;
using nordelta.cobra.service.quotations.Models;
using nordelta.cobra.service.quotations.Models.InvertirOnline;

namespace nordelta.cobra.service.quotations.Services.Contracts
{
    public interface IQuotationService
    {
        public Task<List<Titulo>> GetQuotationsAsync();
        public Task<bool> InformMepQuotationAsync(DolarMEP quotation);

        public Task<List<QuotationExternal>> GetQuotationsFromBcuAsync(DateTime ? date = null);
        public Task<List<QuotationExternal>> GetQuotationsFromBnaAsync(DateTime? date = null);
        public Task<QuotationExternal> GetUsdMayoristaFromBcraAsync();
        public Task<List<QuotationExternal>> GetQuotationsFromCoinApiAsync(DateTime? date = null);

        public Task<bool> InformQuotationAsync(QuotationExternal quotation, string endpoint);
        public Task<bool> InformQuotationsAsync(List<QuotationExternal> quotations, string endpoint);

        public Task<QuotationExternal> GetCacAsync();
        Task GetDolarMepValueAsync();
        Task<List<BonoConfig>> GetBonosConfigAsync(string endpoint);
        Task<List<QuotationExternal>> GetSourceQuotationsAsync(DateTime date, IEnumerable<string> quotes);
        Task<List<BonoConfig>> GetEspeciesAsync();

    }
}
