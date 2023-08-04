using nordelta.cobra.webapi.Controllers.ViewModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace nordelta.cobra.webapi.Services.Contracts
{
    public interface IUserService
    {
        Task SyncSsoUsers();
        IEnumerable<string> GetEmailsForRole(string role);
        Task SyncForeignCuits();
    }
}
