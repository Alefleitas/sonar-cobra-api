using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace nordelta.cobra.webapi.Services.DTOs
{
    public class ResumenCuentaRequest
    {
        public string TipoDocumento { get; set; }
        public List<string> NroDocumentos { get; set; }

        public ResumenCuentaRequest()
        {
            this.NroDocumentos = new List<string>();
        }
	}
}
