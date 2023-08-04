using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using DebitoInmediatoServiceItau;
using Microsoft.Extensions.Options;
using nordelta.cobra.webapi.Configuration;

namespace nordelta.cobra.webapi.Connected_Services.Itau.DebitoInmediatoServiceItau
{
    public class DebitoInmediatoServiceItau : DebitoInmediatoClient
    {
        public DebitoInmediatoServiceItau(IOptions<ItauWCFConfiguration> configuration) : base(EndpointConfiguration.DebitoInmediatoSOAP, configuration.Value.EndpointUrl)
        {
        }
    }
}
