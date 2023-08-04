using System;
using System.Collections.Generic;
using nordelta.cobra.webapi.Models;

namespace nordelta.cobra.webapi.Services.Contracts
{
    public interface IRestrictionsListService
    {
        bool SetLockAdvancePayments(bool action, User user);
        LockAdvancePayments GetLockAdvancePayments();
        List<Restriction> GetCompleteRestrictionsList();
        List<Restriction> GetRestrictionsListByUserId(string userId);
        bool PostRestrictionList(List<Restriction> restrictions);
        bool DeleteRestrictionsByUserId(string userId);
    }
}