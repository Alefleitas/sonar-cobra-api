using System;

namespace nordelta.cobra.webapi.Controllers
{
    public class CommunicationFromServiceModel
    {
        public string Product { get; set; }
        public DateTimeOffset Date { get; set; }

        public string Subject { get; set; }

        public string Body { get; set; }

        public string Sender { get; set; }

        public string Receiver { get; set; }

        public string MessageId { get; set; }
    }
}
