using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace nordelta.cobra.webapi.Services.DTOs
{
    public class GetDebinStateRequest
    {
        public string CodigoDebin { get; set; }
        public string Cbu { get; set; }
        public string Cuit { get; set; }
    }
}
