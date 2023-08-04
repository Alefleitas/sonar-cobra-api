namespace nordelta.cobra.service.quotations.Models
{
    public class Dolar : Quotation
    {
        public Dolar()
        {
            base.RateType = RateTypes.Usd;
            base.FromCurrency = "USD";
            base.ToCurrency = "ARS";
        }
        public string Especie { get; set; }
        public override double Calcular()
        {
            return base.Valor;
        }
    }
}
