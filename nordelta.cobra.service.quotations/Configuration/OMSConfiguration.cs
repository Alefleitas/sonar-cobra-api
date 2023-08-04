using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace nordelta.cobra.service.quotations.Configuration
{
    public class OMSConfiguration
    {
        public string Username { get; set; }
        public string Password { get; set; }
        public string oauthBaseUsername { get; set; }
        public string oauthBasePassword { get; set; }
        public string ServerUrl { get; set; }
    }
}
