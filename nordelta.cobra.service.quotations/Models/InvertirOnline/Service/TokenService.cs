using Microsoft.Extensions.Options;
using nordelta.cobra.service.quotations.Configuration;
using nordelta.cobra.service.quotations.Models.InvertirOnline.Service.Contract;
using nordelta.cobra.service.quotations.Utils;
using ServiceStack;

namespace nordelta.cobra.service.quotations.Models.InvertirOnline.Helper
{
    public class TokenService: ITokenService
    {
        private readonly IOptions<InvertirOnlineConfiguration> _invertirOnline;
        private readonly IConfiguration _configuration;
        private readonly ILogger<TokenService> _logger;
        private string _token;

        public TokenService(IOptions<InvertirOnlineConfiguration> invertirOnline, IConfiguration configuration, ILogger<TokenService> logger)
        {
            _invertirOnline = invertirOnline;
            _configuration = configuration;
            _logger = logger;
        }

        public async Task<string> GetTokenAsync()
        {
            if (_token == null)
            {
                _token = await LoginAsync();
            }
            return _token;
        }


        private async Task<string> LoginAsync()
        {
            var token = string.Empty;
            try
            {
                _logger.LogDebug("Starting call to LoginAsync...");
                var url = _invertirOnline.Value.Url
                    .AppendPath("token");

                var loginRequest = await url
                    .PostToUrlAsync(new
                    {
                        username = _invertirOnline.Value.User,
                        password = AesManager.GetPassword(_invertirOnline.Value.Pass, _configuration.GetSection("SecretKey").Value),
                        grant_type = "password"
                    });
                var loginResponse = loginRequest.FromJson<Token>();

                return loginResponse.access_token;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error in LoginAsync");

                return token;
            }
        }
    }


}
