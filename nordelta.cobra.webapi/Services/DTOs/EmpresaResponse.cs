using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace nordelta.cobra.webapi.Services.DTOs
{
    public class EmpresaResponse
    {
        public string Id { get; set; }
        public string IdBusinessUnit { get; set; }
        public string Nombre { get; set; }
        public string Firma { get; set; }
        public string Correo { get; set; }
    }
}
