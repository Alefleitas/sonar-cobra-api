using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace nordelta.cobra.webapi.Models.MessageChannel;

public interface IMessageChannel<IMessage>
{
    void SubscribeForIncoming(IMessageChannelObserver observer);
    Task SendMessageAsync(IMessage message);
    Task ListenForIncomingAsync(DateTime? dateTime, int retryNumber = 0);
    void SendEmailQuotationBot(string details, IEnumerable<string> recipients = null, Exception ex = null);
}
