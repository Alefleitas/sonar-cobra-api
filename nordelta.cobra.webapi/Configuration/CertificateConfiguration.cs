namespace nordelta.cobra.webapi.Configuration
{
    public class CertificateConfiguration
    {
        public const string GaliciaCertificates = "GaliciaCertificates";

        public const string PublicKey = "PublicKey";
        public const string PrivateKey = "PrivateKey";

        public string Name { get; set; }
        public string FileName { get; set; }
        public string Password { get; set; }
    }
}
