using nordelta.cobra.webapi.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace nordelta.cobra.webapi.Repositories.Contracts
{
    public interface IUserChangesLogRepository
    {
        IUnitOfWork UnitOfWork { get; }
        void Add(UserChangesLog userChanges);
        void AddRange(List<UserChangesLog> userChangesLog);
    }
}
