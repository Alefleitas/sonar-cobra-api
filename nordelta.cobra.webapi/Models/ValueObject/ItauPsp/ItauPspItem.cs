using nordelta.cobra.webapi.Services.DTOs;
using System.Text.Json.Serialization;
using Newtonsoft.Json.Converters;

namespace nordelta.cobra.webapi.Models.ValueObject.ItauPsp
{
    public class ItauPspItem
    {
        public const string ItauPspItems = "ItauPspItems";

        public const string ServicioDeCobranzas = "100";
        public const string ServicioDeCvu = "160";

        public string Name { get; set; }
        public string VendorCuit { get; set; }
        public string ClientId { get; set; }
        public string ProductoNumero { get; set; }
        public string ConvenioNumero { get; set; }

        [JsonConverter(typeof(StringEnumConverter))]
        public PersonType PersonType { get; set; }
        public string CodigoOrganismo { get; set; }

    }
}
