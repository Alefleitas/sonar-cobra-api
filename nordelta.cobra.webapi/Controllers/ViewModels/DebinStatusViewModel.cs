
using nordelta.cobra.webapi.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace nordelta.cobra.webapi.Controllers.ViewModels
{
    public class DebinStatusViewModel
    {
        //Get VendedorCBU with this cuit
        public PaymentStatus Codigo { get; set; }
        public string Descripcion { get; set; }
    }
}
