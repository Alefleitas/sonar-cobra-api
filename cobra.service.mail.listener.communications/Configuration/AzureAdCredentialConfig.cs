namespace cobra.service.mail.listener.communications.Configuration;

public class AzureAdCredentialConfig
{
    public const string AzureAdEmailListener = "AzureAdEmailListener";

    public string RequesUri { get; set; } = string.Empty;
    public string ClientId { get; set; } = string.Empty;
    public string TenantId { get; set; } = string.Empty;
    public string ClientSecret { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string Password { get; set; } = string.Empty;
}
