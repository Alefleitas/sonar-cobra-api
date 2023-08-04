namespace cobra.service.mail.listener.communications.Configuration;

public class EmailListenerConfig
{
    public const string EmailListenerImap = "EmailListenerImap";
    public const string EmailListenerSmtp = "EmailListenerSmtp";

    public string Host { get; set; } = string.Empty;
    public int Port { get; set; }
    public string Email { get; set; } = string.Empty;
    public string Password { get; set; } = string.Empty;
    public string[] Scopes { get; set; }
    public bool EnableSsl { get; set; }
}
