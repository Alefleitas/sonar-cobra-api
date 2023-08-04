using System;
using nordelta.cobra.webapi.Models;

namespace nordelta.cobra.webapi.Services.DTOs
{
    public class AdvanceFeeDto
    {
        public int Id { get; set; }
        public string CodProducto { get; set; }
        public string UserId { get; set; }
        public string ClientCuit { get; set; }
        public string RazonSocial { get; set; }
        public string Solicitante { get; set; }
        public DateTime Vencimiento { get; set; }
        public Currency Moneda { get; set; }
        public float Importe { get; set; }
        public float Saldo { get; set; }
        public EAdvanceFeeStatus Status { get; set; }
        public DateTime CreatedOn { get; set; }
    }
}
