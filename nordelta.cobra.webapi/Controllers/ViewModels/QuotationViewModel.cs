
using nordelta.cobra.webapi.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace nordelta.cobra.webapi.Controllers.ViewModels
{
    public class QuotationViewModel
    {
        public string Code { get; set; }
        public dynamic Data { get; set; }
    }
}
