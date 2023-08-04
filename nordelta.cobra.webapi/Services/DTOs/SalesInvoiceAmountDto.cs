using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace nordelta.cobra.webapi.Services.DTOs
{
    public class SalesInvoiceAmountDto
    {
        public int IdOperacionProducto { get; set; }
        public string Cuit { get; set; }
        public string Codigo { get; set; }
        public decimal Monto { get; set; }
        public string Moneda { get; set; }
    }
}
