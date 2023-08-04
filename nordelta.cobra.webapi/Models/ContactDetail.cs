using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace nordelta.cobra.webapi.Models
{
    [Table("ContactDetails")]
    public class ContactDetail
    {
        public ContactDetail()
        {
            Communications = new HashSet<Communication>();
        }
        [Key]
        public int Id { get; set; }
        [Required]
        public string UserId { get; set; }
        [Required]
        public EComChannelType ComChannel { get; set; }
        [Required]
        public string Value { get; set; }
        [Required]
        public string Description { get; set; }

        public virtual ICollection<Communication> Communications { get; set; }

    }
}
