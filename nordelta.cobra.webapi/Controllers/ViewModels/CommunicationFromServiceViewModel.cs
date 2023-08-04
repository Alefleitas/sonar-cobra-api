using System;

namespace nordelta.cobra.webapi.Controllers.ViewModels
{
    public class CommunicationFromServiceViewModel
    {
        public string Product { get; set; }
        public DateTime Date { get; set; }

        public string Subject { get; set; }

        public string Body { get; set; }

        public string Sender { get; set; }

        public string MessageId { get; set; }
    }
}
