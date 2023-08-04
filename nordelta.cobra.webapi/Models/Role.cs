using Newtonsoft.Json;
using nordelta.cobra.webapi.Repositories.Contexts;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace nordelta.cobra.webapi.Models
{
    [SoftDelete]
    [Auditable]
    public class Role
    {
        [Key]
        [Required]
        public string Name { get; set; }
        public string Description { get; set; }
        [Required]
        public virtual List<Permission> Permissions { get; set; }
        [JsonIgnore]
        public List<NotificationTypeRole> NotificationTypeRoles { get; set; }
    }
}
