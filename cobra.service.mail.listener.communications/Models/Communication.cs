using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace cobra.service.mail.listener.communications.Models
{
    public class Communication
    {
        public string Product { get; set; }

        public DateTime Date { get; set; }

        public string Subject { get; set; }

        public string Body { get; set; }

        public string Sender { get; set; }

        public string MessageId { get; set; }


    }
}
