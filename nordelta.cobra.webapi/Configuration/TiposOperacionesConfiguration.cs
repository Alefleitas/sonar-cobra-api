using System.Collections.Generic;

namespace nordelta.cobra.webapi.Configuration
{
    public class TiposOperacionesConfiguration
    {
        public const string Aplicaciones = "Aplicaciones";
        public const string Saldos = "Saldos";

        public List<string> Operaciones { get; set; }
    }
}
