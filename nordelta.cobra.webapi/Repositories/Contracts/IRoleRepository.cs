using nordelta.cobra.webapi.Models;
using System.Collections.Generic;

namespace nordelta.cobra.webapi.Repositories.Contracts
{
    public interface IRoleRepository
    {
        List<Role> get(List<string> rolesNames = null);

    }
}
