
using Newtonsoft.Json;
using nordelta.cobra.webapi.Repositories.Contexts;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel;
using System;

namespace nordelta.cobra.webapi.Models
{
    [SoftDelete]
    [Auditable]
    public class DashboardQuotation
    {
        [Key]
        public int Id { get; set; }
        [Required]
        [JsonProperty(NullValueHandling = NullValueHandling.Include)]
        public string Description { get; set; }
        [Required]
        public DateTime UploadDate { get; set; }
        [Required]
        public DateTime EffectiveDateFrom { get; set; }
        [Required]
        public DateTime EffectiveDateTo { get; set; }
        [Required]
        public string UserId { get; set; }
        [Required]
        public string RateType { get; set; }
        [Required]
        public string FromCurrency { get; set; }
        [Required]
        public string ToCurrency { get; set; }
        [Required]
        [DefaultValue(0)]
        public double Valor { get; set; }
        [Required]
        public EQuotationSource Source { get; set; }
    }
}
