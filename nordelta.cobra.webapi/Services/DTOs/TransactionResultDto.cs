using System;
using System.Text.Json.Serialization;
using Newtonsoft.Json.Converters;

namespace nordelta.cobra.webapi.Services.DTOs
{
    public class TransactionResultDto
    {
        [JsonPropertyName("transactionId")]
        public string TransactionId { get; set; }

        [JsonPropertyName("transactionType")]
        [JsonConverter(typeof(StringEnumConverter))]
        public TransactionType TransactionType { get; set; }

        [JsonPropertyName("uri")]
        public string Uri { get; set; }

        [JsonPropertyName("status")]
        public TransactionStatusDto Status { get; set; }

        [JsonPropertyName("date")]
        public DateTime Date { get; set; }

        [JsonPropertyName("clientId")]
        public string ClientId { get; set; }

        [JsonPropertyName("cvu")]
        public string Cvu { get; set; }

        [JsonPropertyName("cuit")]
        public string Cuit { get; set; }

        [JsonPropertyName("holderName")]
        public string HolderName { get; set; }

        [JsonPropertyName("personType")]
        public string PersonType { get; set; }

        [JsonPropertyName("currency")]
        public string Currency { get; set; }

        [JsonPropertyName("alias")]
        public string Alias { get; set; }

    }
}
