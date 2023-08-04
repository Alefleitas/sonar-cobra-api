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
    public class DolarMEP : Quotation
    {
        public DolarMEP() {
            base.RateType = RateTypes.UsdMEP;
            base.FromCurrency = "USD";
            base.ToCurrency = "ARS";
        }
        [Required]
        [JsonProperty(NullValueHandling = NullValueHandling.Include)]
        public string Especie { get; set; }
        public override double Calcular()
        {
            return base.Valor;
        }
    }
}
