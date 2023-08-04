using nordelta.cobra.webapi.Repositories.Contexts;
using nordelta.cobra.webapi.Services.DTOs;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace nordelta.cobra.webapi.Models
{
    [SoftDelete]
    [Auditable]
    public class AnonymousPayment
    {
        [Key]
        public int Id { get; set; }
        [Required]
        public string Cuit { get; set; }
        [Required]
        public double Amount { get; set; }
        [Required]
        public Currency Currency { get; set; }
        [Required]
        public PaymentStatus Status { get; set; }
        [Required]
        public PaymentType Type { get; set; }
        public DateTime TransactionDate { get; set; }
        [Required]
        public DateTime IssueDate { get; set; }
        [Required]
        public DateTime ExpirationDate { get; set; }
        [Required]
        public string CBU { get; set; }
        public string VendorCuit { get; set; }
        [Required]
        public string System { get; set; }
        public bool Migrated { get; set; }
        [Required]
        public string DebinCode { get; set; }
    }
}
