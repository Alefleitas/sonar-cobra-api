
using nordelta.cobra.webapi.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace nordelta.cobra.webapi.Controllers.ViewModels
{
    public class OlapQuotationViewModel
    {
        public decimal Valor { get; set; }
        public DateTime Fecha { get; set; }
        public string RateType { get; set; }
    }
}
