using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace nordelta.cobra.webapi.Services.DTOs
{
    public class GetUserResponse
    {
        public string Id { get; set; }
        public string IdApplicationUser { get; set; }
        public string RazonSocial { get; set; }
        public string Cuit { get; set; }
        public string TipoUsuario { get; set; }
    }
}
