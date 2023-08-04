using System;
using System.ComponentModel.DataAnnotations;

namespace nordelta.cobra.webapi.Models
{
    public class PublishedDebtBankFile
    {
        [Key]
        public int Id { get; set; }
        public int ArchivoDeudaId { get; set; }
        public string CuitEmpresa { get; set; }
        public string CodigoOrganismo { get; set; }
        public int NroArchivo { get; set; }
        public string FileName { get; set; }
        public PaymentSource Bank { get; set; }
        public DateTime CreatedOn { get; set; }
    }
}
