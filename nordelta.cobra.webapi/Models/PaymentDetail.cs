using System;
using System.ComponentModel.DataAnnotations.Schema;

namespace nordelta.cobra.webapi.Models
{
    public class PaymentDetail
    {
        public int Id { get; set; }
        public string SubOperationId { get; set; }
        public double Amount { get; set; }
        public string Status { get; set; }
        public string Instrument { get; set; }
        public string ErrorDetail { get; set; }
        public DateTime CreditingDate { get; set; }
        public int PaymentMethodId { get; set; }
        [ForeignKey("PaymentMethodId")]
        public PaymentMethod PaymentMethod { get; set; }
    }
}
