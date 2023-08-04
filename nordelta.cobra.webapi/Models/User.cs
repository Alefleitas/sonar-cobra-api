using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace nordelta.cobra.webapi.Models
{
    [NotMapped]
    public class User
    {
        [Key]
        public string Id { get; set; }
        [Required]
        public string FirstName { get; set; }
        [Required]
        public string LastName { get; set; }
        [Required]
        public long Cuit { get; set; }
        [NotMapped]
        public List<string> AdditionalCuits { get; set; }
        [Required]
        public string Email { get; set; }
        [Required]
        public DateTime BirthDate { get; set; }
        [Required]
        [NotMapped]
        public List<Role> Roles { get; set; }
        [Required]
        [NotMapped]
        public List<BusinessUnit> BusinessUnits { get; set; }
        [NotMapped]
        public List<NotificationUser> NotificationUser { get; set; }
        //[NotMapped]
        public string SupportUserId { get; set; }
        //[NotMapped]
        public string SupportUserName { get; set; }
        public string SupportUserEmail { get; set; }
        public string AccountNumber { get; set; }
        public bool IsForeignCuit { get; set; }
        public string ClientReference { get; set; }

        public User()
        {
            BusinessUnits = new List<BusinessUnit>();
        }
    }
}
