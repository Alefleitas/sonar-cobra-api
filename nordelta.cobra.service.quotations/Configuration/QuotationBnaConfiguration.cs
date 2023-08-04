
namespace nordelta.cobra.service.quotations.Configuration
{
    public class QuotationBnaConfiguration
    {
        public string Url { get; set; }
        public List<TypeQuotation> Types { get; set; }
    }

    public class TypeQuotation
    {
        public string Id { get; set; }
        public List<string> Tables { get; set; }
    }
}
