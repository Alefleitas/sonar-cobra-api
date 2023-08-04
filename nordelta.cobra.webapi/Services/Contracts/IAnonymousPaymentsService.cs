using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using nordelta.cobra.webapi.Controllers.ViewModels;
using nordelta.cobra.webapi.Models;

namespace nordelta.cobra.webapi.Services.Contracts
{
    public interface IAnonymousPaymentsService
    {
        //void checkMigrationsForAnonymousPayments(DateTime date);
        Task<string> PublishExternDebin(ExternDebinViewModel debinData);
        Task<DebinStatusViewModel> GetDebinStatus(string debinCode);
    }
}
