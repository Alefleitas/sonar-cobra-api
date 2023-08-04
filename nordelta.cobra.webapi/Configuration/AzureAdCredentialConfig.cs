namespace nordelta.cobra.webapi.Configuration;

public class AzureAdCredentialConfig
{
    public const string AzureAdQuotationBot = "AzureAdQuotationBot";

    public string RequesUri { get; set; }
    public string ClientId { get; set; }
    public string TenantId { get; set; }
    public string ClientSecret { get; set; }
    public string Email { get; set; }
    public string Password { get; set; }
}
