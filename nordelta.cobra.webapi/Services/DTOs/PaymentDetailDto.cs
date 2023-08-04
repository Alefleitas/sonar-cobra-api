using System;

namespace nordelta.cobra.webapi.Services.DTOs
{
    public class PaymentDetailDto
    {
        public int Id { get; set; }
        public string SubOperationId { get; set; }
        public double Amount { get; set; }
        public string Status { get; set; }
        public string Instrument { get; set; }
        public string ErrorDetail { get; set; }
        public DateTime CreditingDate { get; set; }
    }
}
