using Newtonsoft.Json;
using nordelta.cobra.webapi.Models.ArchivoDeuda;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;

namespace nordelta.cobra.webapi.Models
{
    public abstract class NotificationType
    {
        [Key]
        public int Id { get; set; }
        [Required]
        public DeliveryType Delivery { get; set; }
        [Required]
        public string Description { get; set; }
        public Template Template { get; set; }
        public int ConfigurationDays { get; set; }
        public string CronExpression { get; set; }
        [NotMapped]
        [Required]
        public List<Role> Roles
        {
            get
            {
                return this.NotificationTypeRoles.Select(x => x.Role).ToList();
            }
            set
            {
                this.NotificationTypeRoles = value.Select(r => new NotificationTypeRole() { Role = r, NotificationType = this }).ToList();
            }
        }
        [JsonIgnore]
        public List<NotificationTypeRole> NotificationTypeRoles { get; set; }
        public abstract Notification Evaluate(List<DetalleDeuda> DetallesDeuda, List<Debin> Debins, List<Communication> communications, List<SsoUser> Users, DateTime date, out Dictionary<string, List<int>> dataMapper);
    }
}
