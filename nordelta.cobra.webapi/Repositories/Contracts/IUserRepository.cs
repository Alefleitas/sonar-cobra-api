using nordelta.cobra.webapi.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace nordelta.cobra.webapi.Repositories.Contracts
{
    public interface IUserRepository
    {
        Task AddUserRangeAsync(IEnumerable<SsoUser> ssoUsers);
        void RemoveUsers();
        bool HasUsers();
        List<SsoUser> GetAllUsers(); //All Cobra users
        SsoUser GetSsoUserByCuit(string cuit);
        User GetUserByCuit(string cuit);
        User GetUserById(string id);
        SsoUser GetSsoUserById(string id);
        List<SsoUser> GetUsersByCuits(List<string> cuits);
        List<SsoUser> GetUsersByIds(List<string> ids);
        List<SsoUser> GetUsersByRoles(List<string> roles);
        IEnumerable<string> GetEmailsForRole(string role);
        List<string> FindUserIdsByName(string name);
        List<SsoUser> GetUsersDataByCuits(List<string> cuits);
        List<SsoUserCuit> FindUsersDataByName(string name);
        List<SsoUser> FindUsersByName(string name);
    }
}
