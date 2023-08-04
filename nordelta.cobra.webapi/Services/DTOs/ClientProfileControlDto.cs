using System.Collections.Generic;

namespace nordelta.cobra.webapi.Services.DTOs
{
    public class ClientProfileControlDto
    {
		public string IdRegistro { get; set; }
		public string NombreCliente { get; set; }
		public string TipoDocumento { get; set; }
		public string NumeroDocumento { get; set; }
		public string NumeroCuenta { get; set; }
		public string NumeroSitio { get; set; }
		public string NombreSitio { get; set; }
		public string DescripcionSitio { get; set; }
		public string Observacion { get; set; }
	}
}
