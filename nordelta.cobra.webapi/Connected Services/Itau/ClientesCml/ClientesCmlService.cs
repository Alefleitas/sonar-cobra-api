using Itau;
using Microsoft.Extensions.Options;
using nordelta.cobra.webapi.Configuration;

namespace nordelta.cobra.webapi.Connected_Services.Itau.ClientesCml
{
    public class ClientesCmlService: CLIENTEClient
    {
        public ClientesCmlService(IOptions<ItauWCFConfiguration> configuration) : base(EndpointConfiguration.CLIENTE_CLIENTEHttpPort, configuration.Value.EndpointUrl)
        {
        }
    }
}
