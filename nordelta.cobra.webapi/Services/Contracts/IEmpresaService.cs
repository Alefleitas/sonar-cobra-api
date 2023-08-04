using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace nordelta.cobra.webapi.Services.Contracts
{
    interface IEmpresaService
    {
        Task SyncSsoEmpresas();
    }
}
