using System.Text.Json.Serialization;

namespace nordelta.cobra.webapi.Services.DTOs;

public class LogDto
{
    [JsonPropertyName("event")]
    public string Event { get; set; }

    [JsonPropertyName("id")]
    public int Id { get; set; }

    [JsonPropertyName("time")]
    public string Time { get; set; }

    [JsonPropertyName("process")]
    public string Process { get; set; }

    [JsonPropertyName("description")]
    public string Description { get; set; }

    [JsonPropertyName("contentfile")]
    public string ContentFile { get; set; }

    [JsonPropertyName("contenttext")]
    public string ContentText { get; set; }
}