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
    public class CAC : Quotation
    {
        public CAC() {
            base.RateType = RateTypes.Cac;
            base.FromCurrency = "CAC";
            base.ToCurrency = "ARS";
        }
        public override double Calcular()
        {
            return base.Valor;
        }
    }
}
