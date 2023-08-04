using nordelta.cobra.webapi.Repositories.Contexts;
using System;
using System.ComponentModel.DataAnnotations;

namespace nordelta.cobra.webapi.Models.ArchivoDeuda
{
    public class PublishedDebtFile: BaseEntity
    {
        [Required]
        public string DebtFileName { get; set; }
        [Required]
        public bool Success { get; set; }
        [Required]
        public DateTime CreatedOn { get; set; }
        public string Error { get; set; }
    }
}
