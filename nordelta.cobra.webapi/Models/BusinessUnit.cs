using System.ComponentModel.DataAnnotations;

namespace nordelta.cobra.webapi.Models
{
    public class BusinessUnit
    {
        [Key]
        [Required]
        public string Name { get; set; }
        //[Required]
        //public string Cuit { get; set; }
    }
}