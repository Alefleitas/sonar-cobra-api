using nordelta.cobra.webapi.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace nordelta.cobra.webapi.Repositories.Contracts
{
    public interface IEmpresaRepository
    {
        bool HasEmpresas();
        void RemoveEmpresas();
        Task AddEmpresaRangeAsync(IEnumerable<SsoEmpresa> ssoEmpresas);
        SsoEmpresa GetByName(string name);

        List<SsoEmpresa> GetAllEmpresas();
    }
}
