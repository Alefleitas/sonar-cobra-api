using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using nordelta.cobra.webapi.Controllers.ViewModels;
using nordelta.cobra.webapi.Models;

namespace nordelta.cobra.webapi.Services.Contracts
{
    public interface ICommunicationService
    {
        Communication InsertOrUpdate(Communication communication, User user);
        List<Communication> GetCommunicationsForAccountBalance(int accountBalanceId);
        bool Delete(int id, User user);
        bool TemplateToggle(int id);

        void HandleCommunicationsFromService(List<CommunicationFromServiceViewModel> emails);

    }
}
