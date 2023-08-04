using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace nordelta.cobra.webapi.Services.DTOs
{
    public class CuitProductBuDto
    {
        public string Cuit { get; set; }
        public string Product { get; set; }
        public string BusinessUnit { get; set; }
        public string ReferenciaCliente { get; set; }
        public string ReferenciaDomicilioCliente { get; set; }
    }
}
