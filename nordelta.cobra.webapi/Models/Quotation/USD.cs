using nordelta.cobra.webapi.Repositories.Contexts;

namespace nordelta.cobra.webapi.Models
{
    [SoftDelete]
    [Auditable]
    public class USD : Quotation
    {
        public USD()
        {
            FromCurrency = "USD";
            ToCurrency = "ARS";
        }

        public override double Calcular()
        {
            return base.Valor;
        }
    }
}
