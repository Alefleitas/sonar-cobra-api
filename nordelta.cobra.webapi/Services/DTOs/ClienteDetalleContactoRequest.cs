using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace nordelta.cobra.webapi.Services.DTOs
{
    public class ClienteDetalleContactoRequest 
    { 
        public List<string> NroDocumentos { get; set; }

        public ClienteDetalleContactoRequest()
        {
            this.NroDocumentos = new List<string>();
        }
	}
}
