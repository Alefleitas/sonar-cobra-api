
namespace nordelta.cobra.service.quotations.Configuration
{
    public class QuotationBcuConfiguration
    {
        public string Url { get; set; }
        public List<Monedas> Monedas { get; set; }
    }

    public class Monedas
    {
        public string Val { get; set; }
        public string Text { get; set; }
    }
}
