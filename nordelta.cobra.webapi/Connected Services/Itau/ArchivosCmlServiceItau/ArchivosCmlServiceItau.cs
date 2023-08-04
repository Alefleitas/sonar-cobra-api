using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using ArchivosCmlServiceItau;
using Microsoft.Extensions.Options;
using nordelta.cobra.webapi.Configuration;

namespace nordelta.cobra.webapi.Connected_Services.Itau.ArchivosCmlServiceItau
{
    public class ArchivosCmlServiceItau : ARCHIVOSCMLClient
    {
        public ArchivosCmlServiceItau(IOptions<ItauWCFConfiguration> configuration) : base(EndpointConfiguration.ARCHIVOS_ARCHIVOSCMLHttpPort, configuration.Value.EndpointUrl)
        {
        }
    }
}
