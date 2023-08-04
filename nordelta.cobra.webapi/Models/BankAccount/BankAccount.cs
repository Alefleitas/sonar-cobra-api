using nordelta.cobra.webapi.Repositories.Contexts;
using System.ComponentModel.DataAnnotations;

namespace nordelta.cobra.webapi.Models
{
    [SoftDelete]
    [Auditable]
    public class BankAccount
    {
        [Key]
        public int Id { get; set; }

        [Required]
        public string Cbu { get; set; }

        /// <summary>
        /// BankAccount CUIT number. It can be different than CUIT from BankAccount's associated Client
        /// </summary>
        [Required]
        public string Cuit { get; set; }

        //[Required]
        //public Client Client { get; set; }
        [Required] public string ClientCuit { get; set; }
        public string ClientAccountNumber { get; set; }

        [Required]
        public BankAccountStatus Status { get; set; }

        [Required]
        public Currency Currency { get; set; } 
    }
}