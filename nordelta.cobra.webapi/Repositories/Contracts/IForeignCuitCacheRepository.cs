using nordelta.cobra.webapi.Models;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace nordelta.cobra.webapi.Repositories.Contracts;

public interface IForeignCuitCacheRepository
{
    Task AddAsync(IEnumerable<ForeignCuit> ssoUsers);
    List<ForeignCuit> GetAll();
}
