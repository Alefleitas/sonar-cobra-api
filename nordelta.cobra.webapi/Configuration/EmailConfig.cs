namespace nordelta.cobra.webapi.Configuration
{
    public class EmailConfig
    {
        public const string EmailSmtpConfig = "EmailSmtp";
        public const string EmailImapQuotationBotConfig = "EmailImapQuotationBot";
        public const string EmailSmtpQuotationBotConfig = "EmailSmtpQuotationBot";

        public string Host { get; set; }
        public int Port { get; set; }
        public int TimeoutInMinutes { get; set; }

        public string Email { get; set; }
        public string Password { get; set; }
        public bool EnableSsl { get; set; }

        public string[] Scopes { get; set; }
    }
}
