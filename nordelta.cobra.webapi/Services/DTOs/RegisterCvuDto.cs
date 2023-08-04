using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using nordelta.cobra.webapi.Models;

namespace nordelta.cobra.webapi.Services.DTOs
{
    public class RegisterCvuDto
    {
        [JsonProperty("productCode")]
        public string ProductCode { get; set; }
        [JsonProperty("clientId")]
        public string ClientId { get; set; }

        [JsonProperty("cuit")]
        public string Cuit { get; set; }

        [JsonProperty("holderName")]
        public string HolderName { get; set; }

        [JsonProperty("personType")]
        [System.Text.Json.Serialization.JsonConverter(typeof(StringEnumConverter))]
        public PersonType PersonType { get; set; }

        [JsonProperty("currency")]
        [System.Text.Json.Serialization.JsonConverter(typeof(StringEnumConverter))]
        public Currency Currency { get; set; }

    }
}
