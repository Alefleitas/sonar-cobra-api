using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace nordelta.cobra.webapi.Models.ArchivoDeuda
{
    public class DetalleDeuda
    {
        [Key]
        public int Id { get; set; }
        public string TipoRegistro { get; set; }
        public string TipoOperacion { get; set; }
        [MaxLength(200)]
        public string CodigoMoneda { get; set; }
        public string NumeroCliente { get; set; }
        public string TipoComprobante { get; set; }
        [MaxLength(200)]
        public string NroComprobante { get; set; }
        public string NroCuota { get; set; }
        public string NombreCliente { get; set; }
        public string DireccionCliente { get; set; }
        public string DescripcionLocalidad { get; set; }
        public string PrefijoCodPostal { get; set; }
        public string NroCodPostal { get; set; }
        public string UbicManzanaCodPostal { get; set; }
        [MaxLength(200)]
        public string FechaPrimerVenc { get; set; }
        public string ImportePrimerVenc { get; set; }
        public string FechaSegundoVenc { get; set; }
        public string ImporteSegundoVenc { get; set; }
        public string FechaHastaDescuento { get; set; }
        public string ImporteProntoPago { get; set; }
        public string FechaHastaPunitorios { get; set; }
        public string TasaPunitorios { get; set; }
        public string MarcaExcepcionCobroComisionDepositante { get; set; }
        public string FormasCobroPermitidas { get; set; }
        [MaxLength(200)]
        public string NroCuitCliente { get; set; }
        public string CodIngresosBrutos { get; set; }
        public string CodCondicionIva { get; set; }
        public string CodConcepto { get; set; }
        public string DescCodigo { get; set; }
        public string ObsLibrePrimera { get; set; }
        [MaxLength(200)]
        public string ObsLibreSegunda { get; set; }
        public string ObsLibreTercera { get; set; }
        public string ObsLibreCuarta { get; set; } // Codigo de producto
        public string CodigoMonedaTc { get; set; } // Codigo moneda de TC producto
        public string Relleno { get; set; }

        public int ArchivoDeudaId { get; set; }
        [ForeignKey("ArchivoDeudaId")]
        public ArchivoDeuda ArchivoDeuda { get; set; }
        public int? PaymentMethodId { get; set; }
        [ForeignKey("PaymentMethodId")]
        public PaymentMethod PaymentMethod { get; set; }
        public int? PaymentReportId { get; set; }
        [ForeignKey("PaymentReportId")]
        public PaymentReport PaymentReport { get; set; }
    }
}
