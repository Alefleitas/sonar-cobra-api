namespace nordelta.cobra.webapi.Services.DTOs
{
    public class ResumenCuentaResponse
    {
        public string Orden { get; set; }
        public string Cuit { get; set; }
        public string IdCuentaCorriente { get; set; }
        public string Fecha { get; set; }
        public string ComprobanteTipo { get; set; }
        public string Comprobante { get; set; }
        public string Moneda { get; set; }
        public string Debe { get; set; }
        public string Haber { get; set; }
        public string Saldo { get; set; }
        public string Unidad { get; set; }
        public string TipoOperacion { get; set; }
        public string ReciboTipo { get; set; }
        public string ReciboImporte { get; set; }
        public string ReciboImporteAplicado { get; set; }
        public string Intereses { get; set; }
        public string Capital { get; set; }
        public string TrxId { get; set; }
        public string FacElect { get; set; }
        public string TrxNumber { get; set; }
        public string Producto { get; set; }
        public bool? OnDebtDetail { get; set; }
        public bool? Processing { get; set; }
        public bool? AutoApproved { get; set; }
    }
}