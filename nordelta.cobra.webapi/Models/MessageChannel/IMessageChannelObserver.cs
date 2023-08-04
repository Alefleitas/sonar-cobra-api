using nordelta.cobra.webapi.Models.MessageChannel.Messages;
using System.Threading.Tasks;

namespace nordelta.cobra.webapi.Models.MessageChannel;

public interface IMessageChannelObserver
{
    Task NotifyIncomingMessageAsync(IMessage message, IMessageChannel<IMessage> messageChannel);
}