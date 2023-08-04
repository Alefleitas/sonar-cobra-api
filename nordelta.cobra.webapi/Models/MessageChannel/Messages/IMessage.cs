using System;

namespace nordelta.cobra.webapi.Models.MessageChannel.Messages
{
    public interface IMessage
    {
        public string From { get; set; }
        public string To { get; set; }
        public DateTime DateTimeSent { get; set; }

        public string Text()
        {
            return $@"From: {From}
                    To: {To}
                    Date: {DateTimeSent:dd/MMMM/yyyy HH:mm}
                    {FullMessage()}";
        }
        public abstract string FullMessage();
        public abstract IMessage GetResponseObject(string response);

    }
}