using nordelta.cobra.webapi.Repositories.Contexts;
using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace nordelta.cobra.webapi.Models
{
    [SoftDelete]
    [Auditable]
    public class Debin : PaymentMethod
    {
        public Debin()
        {
            base.OlapMethod = "EF";
            base.Instrument = PaymentInstrument.DEBIN;
        }

        [Required]
        public string DebinCode { get; set; }
        [Required]
        public DateTime IssueDate { get; set; }
        [Required]
        public DateTime ExpirationDate { get; set; }
        [Required]
        public int BankAccountId { get; set; }
        [ForeignKey("BankAccountId")]
        public BankAccount BankAccount { get; set; }

        public string VendorCuit { get; set; }
        public string ExternalCode { get; set; }

    }
}
