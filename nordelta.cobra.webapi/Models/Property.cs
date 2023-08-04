using System.ComponentModel.DataAnnotations;

namespace nordelta.cobra.webapi.Models
{
    public class Property
    {
        [Key]
        public int LotNumber { get; set; }
        [Required]
        public double Price { get; set; }
        [Required]
        public Currency Currency { get; set; }
    }
}
