using nordelta.cobra.webapi.Models;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace nordelta.cobra.webapi.Controllers.ViewModels
{
    public class OrderAdvanceFeeViewModel
    {
        public string Cuit { get; set; }
        public string CodProducto { get; set; }
        public DateTime Vencimiento { get; set; }
        public Currency Moneda{ get; set; }
        public float Importe { get; set; }
        public float Saldo { get; set; }
    }
}
