namespace nordelta.cobra.webapi.Services.DTOs
{
    public class UpdateUserRequest
    {
        public string SsoToken { get; set; }
        public string SocialReason { get; set; }
        public string Cuit { get; set; }
    }
}
