using System;
using System.Text.Json.Serialization;
using Newtonsoft.Json.Converters;
using nordelta.cobra.webapi.Models;

namespace nordelta.cobra.webapi.Services.DTOs
{
    public class CvuDto
    {
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
        [JsonConverter(typeof(StringEnumConverter))]
        public Currency Currency { get; set; }

        [JsonPropertyName("alias")]
        public string Alias { get; set; }

    }

    public class OriginatorDto
    {
        [JsonPropertyName("name")]
        public string Name { get; set; }

        [JsonPropertyName("cbu")]
        public string Cbu { get; set; }

        [JsonPropertyName("cuit")]
        public string Cuit { get; set; }

        [JsonPropertyName("bank")]
        public string Bank { get; set; }

        [JsonPropertyName("branch")]
        public string Branch { get; set; }

        [JsonPropertyName("alias")]
        public string Alias { get; set; }

    }

    public class OperationInformationResultDto
    {
        [JsonPropertyName("operationId")]
        public string OperationId { get; set; }

        [JsonPropertyName("coelsaId")]
        public string CoelsaId { get; set; }

        [JsonPropertyName("fecha_negocio")]
        public string FechaNegocio { get; set; }

        [JsonPropertyName("cvu")]
        public CvuDto Cvu { get; set; }

        [JsonPropertyName("amount")]
        public string Amount { get; set; }

        [JsonPropertyName("originator")]
        public OriginatorDto Originator { get; set; }

        [JsonPropertyName("codigoOrganismo")]
        public string CodigoOrganismo { get; set; }
    }
}
