using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace cobra.service.mail.listener.communications.Configuration
{
    public class EmailSenderConfig
    {
        public EmailSenderConfig()
        {
            ValidDomains = new List<string>();
        }
        public List<string> ValidDomains { get; set; }
    }
}
