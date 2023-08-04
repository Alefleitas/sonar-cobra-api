using System.ComponentModel.DataAnnotations;

namespace nordelta.cobra.webapi.Models
{
    public class LockAdvancePayments
    {
        [Key]
        public int Id { get; set; }
        [Required]
        public bool LockedByUser { get; set; }
    }
}