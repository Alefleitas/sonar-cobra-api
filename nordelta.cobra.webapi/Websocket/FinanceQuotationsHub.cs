using Microsoft.AspNetCore.SignalR;
using nordelta.cobra.webapi.Services.Contracts;
using System.Threading.Tasks;

namespace nordelta.cobra.webapi.Websocket
{

    public class FinanceQuotationsHub : Hub
    {
        private readonly IFinanceQuotationsService _financeQuotationsService;

        public FinanceQuotationsHub(IFinanceQuotationsService financeQuotationsService)
        {
            _financeQuotationsService = financeQuotationsService;
        }

        // Envía data al cliente cuando se efectúa la conexión
        public override async Task OnConnectedAsync()
        {
            await Clients.Caller.SendAsync(WebsocketCommands.SendQuotations, _financeQuotationsService.GetQuotations());
            await base.OnConnectedAsync();
        }
    }
}

