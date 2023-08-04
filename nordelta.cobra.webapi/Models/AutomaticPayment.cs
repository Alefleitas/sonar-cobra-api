using nordelta.cobra.webapi.Repositories.Contexts;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace nordelta.cobra.webapi.Models
{
    [SoftDelete]
    [Auditable]
    public class AutomaticPayment
    {
        [Key]
        public int Id { get; set; }

        [Required]
        public User Payer { get; set; }

        [Required]
        public Currency Currency { get; set; }

        [Required]
        public BankAccount BankAccount { get; set; }

        /// <summary>
        /// References the NroComprobante field on DetalleDeuda
        /// </summary>
        [Required]
        public string Product { get; set; }
    }
}
