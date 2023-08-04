using Newtonsoft.Json;
using nordelta.cobra.webapi.Repositories.Contexts;
using System;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace nordelta.cobra.webapi.Models
{
    [SoftDelete]
    [Auditable]
    public abstract class Quotation
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
        [NotMapped]
        public bool? StoredInDb { get; set; }
        public abstract double Calcular();
    }

    public class QuotationNotificationUser
    {
        public string Id { get; set; }
        public string Email { get; set; }
    }

    public class RateTypes
    {
        public const string
            BcraMayor = "BCRA Mayorista Com. A3500",
            BillBcuComprador = "Billete BCU Comprador",
            BillBcuCompradorContable = "Billete BCU Comprador Contable",
            BillBcuVendedor = "Billete BCU Vendedor",
            BillBcuVendedorContable = "Billete BCU Vendedor Contable",
            BillBnaComprador = "Billete BNA Comprador",
            BillBnaCompradorContable = "Billete BNA Comprador Contable",
            BillBnaVendedor = "Billete BNA Vendedor",
            BillBnaVendedorContable = "Billete BNA Vendedor Contable",
            Corporate = "Corporate",
            DivBnaComprador = "Divisa BNA Comprador",
            DivBnaCompradorContable = "Divisa BNA Comprador Contable",
            DivBnaVendedor = "Divisa BNA Vendedor",
            DivBnaVendedorContable = "Divisa BNA Vendedor Contable",
            Fixed = "Fixed",
            Promedio = "Promedio",
            Spot = "Spot",
            UsdMEP = "USD MEP",
            UvaBcra = "UVA BCRA",
            Cac = "CAC",
            Cryptos = "Cryptos";
    }

    public enum EQuotationSource
    {
        MANUAL,
        RAVA,
        CAMARCO,
        BYMA,
        ARCHIVO_PUBLICACION,
        OLAP,
        BCRA,
        COBRA,
        INVERTIRONLINE,
        COINAPI,
        BCU,
        BNA
        
    }
}
