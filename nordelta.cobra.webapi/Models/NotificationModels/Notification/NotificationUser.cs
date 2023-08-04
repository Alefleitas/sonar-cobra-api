using System.ComponentModel.DataAnnotations;

namespace nordelta.cobra.webapi.Models
{
    public class NotificationUser
    {
        [Key]
        public int Id { get; set; }
        [Required]
        public User User { get; set; }
        [Required]
        public Notification Notification { get; set; }
    }
}
