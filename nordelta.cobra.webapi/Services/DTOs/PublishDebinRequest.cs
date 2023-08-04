namespace nordelta.cobra.webapi.Services.DTOs
{
    public class PublishDebinRequest
    {
        public string CompradorCuit { get; set; }
        public string CompradorCbu { get; set; }
        public string CompradorRazonSocial { get; set; }
        public string VendedorCbu { get; set; }
        public string VendedorCuit { get; set; }

        public string Comprobante { get; set; }
        public string FechaVencimiento { get; set; }
        public string HoraVencimiento { get; set; }
        public double Importe { get; set; }
        public string Moneda { get; set; }
        public string Concepto { get; set; }

    }
}
