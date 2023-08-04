using nordelta.cobra.webapi.Repositories.Contexts;

namespace nordelta.cobra.webapi.Models
{
    [SoftDelete]
    [Auditable]
    public class USDUYU : Quotation
    {
        public USDUYU()
        {
            base.FromCurrency = "USD";
            base.ToCurrency = "UYU";
        }

        public override double Calcular()
        {
            return base.Valor;
        }
    }
}
