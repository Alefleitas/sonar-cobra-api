using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace nordelta.cobra.webapi.Configuration
{
    public class CustomItauCvuConfiguration
    {
        public const string CustomItauCvuConfig = "CustomItauCvuConfig";

        public bool EnableCustomItauCvu { get; set; }
        public List<EnabledBU> EnabledBUs { get; set; }

    }

    public class EnabledBU
    {
        public string BusinessUnit { get; set; }
        public List<EnabledAccountBalance> EnabledAccountBalances { get; set; }
    }

    public class EnabledAccountBalance
    {
        public string ClientCuit { get; set; }
        public string Product { get; set; }
    }

}
