using nordelta.cobra.webapi.Repositories.Contexts;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace nordelta.cobra.webapi.Models
{
    [SoftDelete]
    [Auditable]
    public class UVAUSD : Quotation
    {
        public UVAUSD()
        {
            base.RateType = RateTypes.UsdMEP;
            base.FromCurrency = "UVA";
            base.ToCurrency = "USD";
        }
        public override double Calcular()
        {
            return base.Valor;
        }
    }
}
