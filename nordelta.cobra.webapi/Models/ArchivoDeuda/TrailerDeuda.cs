using System.ComponentModel.DataAnnotations;

namespace nordelta.cobra.webapi.Models.ArchivoDeuda
{
    public class TrailerDeuda
    {
        [Key]
        public int Id { get; set; }
        public string TipoRegistro { get; set; }
        public string ImporteTotalPrimerVencimiento { get; set; }
        public string Ceros { get; set; }
        public string CantRegistroInformados { get; set; }
        public string Relleno { get; set; }
    }
}
