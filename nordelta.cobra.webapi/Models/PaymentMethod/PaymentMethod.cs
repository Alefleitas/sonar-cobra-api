using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Runtime.Serialization;
using System.Text.Json.Serialization;
using nordelta.cobra.webapi.Models.ArchivoDeuda;
using nordelta.cobra.webapi.Repositories.Contexts;

namespace nordelta.cobra.webapi.Models
{
    [SoftDelete]
    [Auditable]
    public abstract class PaymentMethod
    {
        [Key]
        public int Id { get; set; }
        [Required]
        public User Payer { get; set; }
        [Required]
        public double Amount { get; set; }
        [Required]
        public Currency Currency { get; set; }
        [Required]
        public List<DetalleDeuda> Debts { get; set; }
        [Required]
        public DateTime TransactionDate { get; set; }
        public PaymentSource Source { get; set; }
        public PaymentStatus Status { get; set; }
        public PaymentInstrument Instrument { get; set; }
#nullable enable
        public PaymentType? Type { get; set; }
        public string? OperationId { get; set; }
        public string? OlapAcuerdo { get; set; }
        public string? OlapMethod { get; set; }
        public string? LockboxName { get; set; }
        public DateTime? InformedDate { get; set; }

        public List<PaymentDetail>? PaymentDetails { get; set; }
    }
}
