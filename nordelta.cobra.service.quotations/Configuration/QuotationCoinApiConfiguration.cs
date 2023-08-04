
namespace nordelta.cobra.service.quotations.Configuration
{
    public class QuotationCoinApiConfiguration
    {
        public string ApiKey { get; set; }
        public List<ExchangeRate> ExchangeRates { get; set; }
    }

    public class ExchangeRate
    {
        public string BaseId { get; set; }
        public string QuoteId { get; set; }
    }
}
