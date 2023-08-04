namespace nordelta.cobra.webapi.Models.ArchivoDeuda
{
    public class PropertyCode
    {
        public string NroComprobante { get; set; }
        public string CuitEmpresa { get; set; }
        public string RazonSocial { get; set; }
        public string NroCuitCliente { get; set; }
        public string AccountNumber { get; set; }
        public string ProductCode { get; set; }
        public string Emprendimiento { get; set; }
        public string BusinessUnit { get; set; }
        public string ReferenciaCliente { get; set; }
    }

    public class PropertyCodeFull
    {
        public string NroComprobante { get; set; }
        public string CuitEmpresa { get; set; }
        public string NroCuitCliente { get; set; }
        public string FormatedFileName { get; set; }
        public string TimeStamp { get; set; }
        public string ProductCode { get; set; }
        public string Emprendimiento { get; set; }
    }
}
