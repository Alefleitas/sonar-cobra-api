using System;

namespace nordelta.cobra.webapi.Controllers.ViewModels
{
    public class RepeatedDebsDetailsViewModel
    {
        public int Id { get; set; }
        public string NroComprobante { get; set; }
        public DateTime FechaPrimerVenc { get; set; }
        public string CodigoMoneda { get; set; }
        public string CodigoProducto { get; set; }
        public string CodigoTransaccion { get; set; }
        public string RazonSocialCliente { get; set; }
        public string NroCuitCliente { get; set; }
        public string IdClienteOracle { get; set; }
        public string IdSiteOracle { get; set; }
    }
}