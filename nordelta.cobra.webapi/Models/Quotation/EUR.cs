using nordelta.cobra.webapi.Repositories.Contexts;

namespace nordelta.cobra.webapi.Models
{
    [SoftDelete]
    [Auditable]
    public class EUR : Quotation
    {
        public EUR()
        {
            FromCurrency = "EUR";
            ToCurrency = "ARS";
        }

        public override double Calcular()
        {
            return base.Valor;
        }
    }
}
