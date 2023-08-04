using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;

namespace nordelta.cobra.webapi.Models
{
    public class Notification
    {
        public int Id { get; set; }
        public DateTime Date { get; set; }
        [NotMapped]
        [Required]
        public List<User> Recipients {
            get
            {
                return this.NotificationUserList.Select(x => x.User).ToList();
            }
            set
            {
                this.NotificationUserList = value.Select(u => new NotificationUser() { User = u, Notification = this }).ToList();
            }
        }
        public List<NotificationUser> NotificationUserList { get; set; }
        public NotificationType NotificationType { get; set; }
        public string Product { get; set; }
    }
}
