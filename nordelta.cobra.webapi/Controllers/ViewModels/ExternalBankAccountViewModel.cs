using System.ComponentModel.DataAnnotations;

namespace nordelta.cobra.webapi.Controllers.ViewModels
{
    public class ExternalBankAccountViewModel
    {
        [Required]
        [StringLength(22)]
        [RegularExpression("^[0-9]*$", ErrorMessage = "Cbu invalido")]
        public string Cbu { get; set; }

        [Required]
        [StringLength(22)]
        [RegularExpression("^[0-9]*$", ErrorMessage = "Cuit invalido")]
        public string Cuit { get; set; }

        public string Moneda { get; set; }
    }
}
