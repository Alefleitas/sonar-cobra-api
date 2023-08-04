using System.ComponentModel.DataAnnotations;

namespace nordelta.cobra.webapi.Models.ArchivoDeuda
{
    public class OrganismoDeuda
    {
        [Key]
        public int Id { get; set; }
        public string CuitEmpresa { get; set; }
        public string NroDigitoEmpresa { get; set; }
        public string CodProducto { get; set; }
        public string NroAcuerdo { get; set; }
    }
}
