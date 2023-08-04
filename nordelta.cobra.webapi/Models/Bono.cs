using nordelta.cobra.webapi.Repositories.Contexts;
using System.ComponentModel.DataAnnotations;

namespace nordelta.cobra.webapi.Models
{
    [SoftDelete]
    [Auditable]
    public class Bono
    {
        [Key]
        public int Id { get; set; }
        public string Title { get; set; }
    }
}
