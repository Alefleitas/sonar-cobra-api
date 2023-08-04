using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace nordelta.cobra.webapi.Models
{
    public class NotificationTypeRole
    {
        [Key]
        public int Id { get; set; }
        public Role Role { get; set; }
        public string RoleId { get; set; }
        public NotificationType NotificationType { get; set; }
        public int NotificationTypeId { get; set; }
    }
}
