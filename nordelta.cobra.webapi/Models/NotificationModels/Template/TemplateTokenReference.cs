using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace nordelta.cobra.webapi.Models
{
    public class TemplateTokenReference
    {
        [Key]
        public int Id { get; set; }
        [Required]
        public string Description { get; set; }
        [Required]
        public string Token { get; set; }
        [Required]
        public string ObjectProperty { get; set; }
    }
}