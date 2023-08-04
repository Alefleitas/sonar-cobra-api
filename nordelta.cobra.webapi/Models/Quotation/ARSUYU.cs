using nordelta.cobra.webapi.Repositories.Contexts;

namespace nordelta.cobra.webapi.Models
{
    [SoftDelete]
    [Auditable]
    public class ARSUYU : Quotation
    {
        public ARSUYU()
        {
            base.FromCurrency = "ARS";
            base.ToCurrency = "UYU";
        }

        public override double Calcular()
        {
            return base.Valor;
        }
    }
}
