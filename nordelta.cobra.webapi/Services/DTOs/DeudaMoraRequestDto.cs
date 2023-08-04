using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace nordelta.cobra.webapi.Services.DTOs
{
    public class DeudaMoraRequestDto
    {
        public DeudaMoraRequestDto()
        {
            Cuits = new List<string>();
        }
        public int IdOperacionProducto { get; set; }
        public string CodProducto { get; set; }
        public List<string> Cuits { get; set; }
    }
}
