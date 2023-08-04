using System.ComponentModel.DataAnnotations;

namespace nordelta.cobra.webapi.Models.ArchivoDeuda
{
    public class HeaderDeuda
    {
        [Key]
        public int Id { get; set; }
        public string TipoRegistro { get; set; }
        public string CodOrganismo { get; set; }
        public string CodCanal { get; set; }
        public string NroEnvio { get; set; }
        public string UltimaRendicionProcesada { get; set; }
        public string MarcaActualizacionCuentaComercial { get; set; }
        public string MarcaPublicacionOnline { get; set; }
        public string Relleno { get; set; }
        public OrganismoDeuda Organismo { get; set; }

    }
}
