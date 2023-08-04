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
    public class BTC : Quotation
    {
        public BTC() {
            base.RateType = RateTypes.Cryptos;
            base.FromCurrency = "BTC";
            base.ToCurrency = "USD";
        }
        public override double Calcular()
        {
            return base.Valor;
        }
    }
}
