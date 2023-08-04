using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace nordelta.cobra.webapi.Services.DTOs
{
    public class DeudaMoraResponseDto
    {
        public int IdOperacionProducto { get; set; }
        public string CodProducto { get; set; }
        public string Cuit { get; set; }
        public bool Deuda { get; set; }
        public bool Mora { get; set; }
    }
}
