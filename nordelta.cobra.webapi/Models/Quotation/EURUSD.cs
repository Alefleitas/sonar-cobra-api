using nordelta.cobra.webapi.Repositories.Contexts;
using System;

namespace nordelta.cobra.webapi.Models
{
    [SoftDelete]
    [Auditable]
    public class EURUSD : Quotation
    {
        public EURUSD()
        {
            FromCurrency = "EUR";
            ToCurrency = "USD";
        }

        public override double Calcular()
        {
            return base.Valor;
        }
    }
}
