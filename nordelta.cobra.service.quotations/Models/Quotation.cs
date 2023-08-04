
using nordelta.cobra.service.quotations.Models.Enums;

namespace nordelta.cobra.service.quotations.Models
{
    public abstract class Quotation
    {
        public int Id { get; set; }
        public string Description { get; set; }
        public DateTime UploadDate { get; set; }
        public DateTime EffectiveDateFrom { get; set; }
        public DateTime EffectiveDateTo { get; set; }
        public string UserId { get; set; }
        public string RateType { get; set; }
        public string FromCurrency { get; set; }
        public string ToCurrency { get; set; }
        public double Valor { get; set; }
        public EQuotationSource Source { get; set; }
        public abstract double Calcular();
    }  
}
