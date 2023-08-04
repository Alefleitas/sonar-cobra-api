
using System;

namespace nordelta.cobra.webapi.Models.MessageChannel.Messages
{
    public class EmailMessage : IMessage
    {
        public string Subject { get; set; }
        public string Body { get; set; }
        public string From { get; set; }
        public string To { get; set; }
        public DateTime DateTimeSent { get; set; }

        public string FullMessage()
        {
            return $@"Subject: {Subject}
                    Body: {Body}";
        }

        public IMessage GetResponseObject(string response)
        {
            return new EmailMessage()
            {
                Subject = "RE: " + this.Subject,
                Body = response,
                To = this.From,
                From = this.To,
                DateTimeSent = DateTime.Now
            };
        }
    }
}
