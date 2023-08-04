using Newtonsoft.Json.Converters;
using nordelta.cobra.webapi.Models;
using System;
using System.Text.Json.Serialization;

namespace nordelta.cobra.webapi.Services.DTOs
{
    public class PaymentMethodDto
    {
        public int Id { get; set; }
        public User Payer { get; set; }
        public string OperationId { get; set; }
        public string CuitCliente { get; set; }
        //[JsonConverter(typeof(StringEnumConverter))]
        public Currency Currency { get; set; }
        public double Amount { get; set; }
        public DateTime TransactionDate { get; set; }
        //[JsonConverter(typeof(StringEnumConverter))]
        public PaymentInstrument Instrument { get; set; }
        //[JsonConverter(typeof(StringEnumConverter))]
        public PaymentSource Source { get; set; }
        //[JsonConverter(typeof(StringEnumConverter))]
        public PaymentStatus Status { get; set; }
        //[JsonConverter(typeof(StringEnumConverter))]
        public PaymentType Type { get; set; }
        public string OlapAcuerdo { get; set; }
        public bool HasPaymentDetail { get; set; }
    }
}
