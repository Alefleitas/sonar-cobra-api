
using nordelta.cobra.webapi.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace nordelta.cobra.webapi.Controllers.ViewModels
{
    public class ClientViewModel
    {
        public string Id { get; set; }
        public string Cuit { get; set; }
        public string RazonSocial { get; set; }
    }
}
