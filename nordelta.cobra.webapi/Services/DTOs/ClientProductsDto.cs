using nordelta.cobra.webapi.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace nordelta.cobra.webapi.Services.DTOs
{
    public class ClientProductsDto
    {
        public string BU { get; set; }
        public string Emprendimiento { get; set; }
        //public double MontoTotal { get; set; }
        //public Currency MonedaMontoTotal { get; set; }
        public double TotalPagado { get; set; }
        public Currency MonedaTotalPagado { get; set; }
        public double SaldoPendiente { get; set; }
        public decimal? MontoTotal { get; set; }
        public Currency MonedaSaldoPendiente { get; set; }
        public string Product { get; set; }
    }
}
