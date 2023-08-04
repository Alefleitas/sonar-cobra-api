using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace nordelta.cobra.service.quotations.Models
{
    public class QuotationExternal : Quotation
    {
        public string Especie { get; set; }
        public override double Calcular()
        {
            return base.Valor;
        }
    }
}
