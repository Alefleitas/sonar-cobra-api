using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace nordelta.cobra.webapi.Models
{
    [NotMapped]
    public class BaseEntity: IEntity
    {
        [Key]
        public int Id { get; set; }
    }
}
