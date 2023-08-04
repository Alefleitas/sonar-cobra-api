using System.Threading.Tasks;

namespace nordelta.cobra.webapi.Services.Contracts
{
    public interface IQuotationBotService
    {
        Task ListenAllChannelsAsync();
    }
}
