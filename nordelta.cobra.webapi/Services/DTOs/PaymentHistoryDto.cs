using System.Collections.Generic;

namespace nordelta.cobra.webapi.Services.DTOs
{
    public class PaymentHistoryDto
    {
        public string FechaVenc { get; set; }
        public int NroCuota { get; set; }
        public string Moneda { get; set; }
        public string Importe { get; set; }
        public string Saldo { get; set; }
        public string Producto { get; set; }

        public List<PaymentHistoryDetailDto> Details { get; set; }
    }
}
