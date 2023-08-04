namespace nordelta.cobra.webapi.Configuration
{
    public class ApiServicesConfig
    {
        public const string SgcApi = "SgcApi";
        public const string SgfApi = "SgfApi";
        public const string SsoApi = "SsoApi";
        public const string CobraApi = "CobraApi";
        public const string HolidaysApi = "HolidaysApi";
        public const string ItauApi = "ItauApi";
        public const string QuotationServiceApi = "QuotationServiceApi";
        public const string MiddlewareApi = "MiddlewareApi";

        public string Url { get; set; }
        public string Token { get; set; }
        public string RefreshToken { get; set; }
        public string CertificateName { get; set; }
    }
}
