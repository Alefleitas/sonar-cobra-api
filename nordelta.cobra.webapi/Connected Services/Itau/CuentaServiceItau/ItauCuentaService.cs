using CuentaServiceItau;
using Microsoft.Extensions.Options;
using nordelta.cobra.webapi.Configuration;

namespace nordelta.cobra.webapi.Connected_Services.Itau.CuentaServiceItau
{
    public class ItauCuentaService : CuentaClient
    {
        public ItauCuentaService(IOptions<ItauWCFConfiguration> configuration) : base(EndpointConfiguration.CuentaSOAP, configuration.Value.EndpointUrl)
        {
        }
    }
}
