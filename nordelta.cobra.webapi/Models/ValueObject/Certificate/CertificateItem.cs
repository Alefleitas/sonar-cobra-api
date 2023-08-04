namespace nordelta.cobra.webapi.Models.ValueObject.Certificate
{
    public class CertificateItem
    {
        public const string CertificateItems = "CertificateItems";

        public string VendorName { get; set; }
        public string VendorCuit { get; set; }
        public string  Name { get; set; }
        public string Password { get; set; }

    }
}
