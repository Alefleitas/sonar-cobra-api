using System.Text.Json.Serialization;

namespace nordelta.cobra.webapi.Services.DTOs
{
    public class TransactionStatusCodes
    {
        public const string OrigenPsArchivoConciliacion = "PSOrigenArchivo";
        public const string OrigenCvArchivoConciliacion = "CVOrigenArchivo";
        public const string OkOperationCashIn = "4000";
        public const string OkCreacionCvu = "3200";
    }

    public class TransactionStatusDto
    {
        [JsonPropertyName("code")]
        public string Code { get; set; }

        [JsonPropertyName("message")]
        public string Message { get; set; }
    }
}
