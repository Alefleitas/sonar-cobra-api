using Newtonsoft.Json;
using nordelta.cobra.webapi.Repositories.Contexts;
using System;
using System.ComponentModel.DataAnnotations.Schema;

namespace nordelta.cobra.webapi.Models
{
    [SoftDelete]
    [Auditable]
    public class QuotationExternal : Quotation
    {
        [NotMapped]
        public bool? StoredInDb { get; set; }
        [NotMapped]
        public string? Especie { get; set; }
        public override double Calcular()
        {
            return base.Valor;
        }
    }
}
