using Newtonsoft.Json;
using nordelta.cobra.webapi.Repositories.Contexts;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace nordelta.cobra.webapi.Models
{
    [SoftDelete]
    [Auditable]
    public class CACUSD : Quotation
    {
        public CACUSD() {
            base.RateType = RateTypes.Cac;
            base.FromCurrency = "CAC";
            base.ToCurrency = "USD";
        }
        public override double Calcular()
        {
            return base.Valor;
        }
    }
}
