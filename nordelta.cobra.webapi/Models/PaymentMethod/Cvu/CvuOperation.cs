using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace nordelta.cobra.webapi.Models
{
    public class CvuOperation : PaymentMethod
    {
        [Required]
        public string CoelsaId { get; set; }
        public int CvuEntityId { get; set; }
        [ForeignKey("CvuEntityId")]
        public CvuEntity CvuEntity { get; set; }

        public CvuOperation()
        {
            base.OlapMethod = "CV";
            base.Instrument = PaymentInstrument.CvuOperation;
        }
    }
}
