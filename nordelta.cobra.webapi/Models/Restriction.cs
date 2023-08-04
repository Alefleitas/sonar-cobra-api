using System.ComponentModel.DataAnnotations;

namespace nordelta.cobra.webapi.Models
{
    public class Restriction
    {
        [Key]
        public int Id { get; set; }
        [Required]
        public string UserId { get; set; }
        [Required]
        public EPermission PermissionDeniedCode { get; set; }
    }
}
