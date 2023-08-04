using nordelta.cobra.webapi.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace nordelta.cobra.webapi.Repositories.Contracts
{
    public interface IRestrictionsListRepository
    {
        IUnitOfWork UnitOfWork { get; }
        LockAdvancePayments GetLockAdvancePayments();
        bool SetLockAdvancePayments(bool action, User user);
        List<Restriction> GetCompleteRestrictionsList();
        List<Restriction> GetRestrictionsListByUserId(string userId);
        void AddRestrictions(List<Restriction> restrictions);
        bool DeleteRestrictionsByUserId(string userId);
    }
}
