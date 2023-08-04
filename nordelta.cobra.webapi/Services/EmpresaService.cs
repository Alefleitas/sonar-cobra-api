using nordelta.cobra.webapi.Models;
using nordelta.cobra.webapi.Repositories.Contracts;
using nordelta.cobra.webapi.Services.Contracts;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using Hangfire;
using nordelta.cobra.webapi.Controllers.Helpers;

namespace nordelta.cobra.webapi.Services
{
    public class EmpresaService : IEmpresaService
    {
        protected readonly IEmpresaRepository _empresaRepository;
        protected readonly ILoginService _loginService;
        private readonly IBackgroundJobClient _backgroundJobClient;
        public EmpresaService(IEmpresaRepository empresaRepository, ILoginService loginService, IBackgroundJobClient backgroundJobClient)
        {
            this._empresaRepository = empresaRepository;
            this._loginService = loginService;
            this._backgroundJobClient = backgroundJobClient;
        }
        [DisableConcurrentExecution(timeoutInSeconds: 1800)]
        public async Task SyncSsoEmpresas()
        {
            List<SsoEmpresa> ssoEmpresas = new List<SsoEmpresa>();
            Debug.WriteLine("Get all empresas from SSO.");
            var empresasResponse = _loginService.GetSsoEmpresas();
            if (empresasResponse != null && empresasResponse.Any())
            {
                Debug.WriteLine("Sso empresas count: " + empresasResponse.Count);
                try
                {
                    ssoEmpresas.AddRange(empresasResponse.Select(empresa => new SsoEmpresa
                    {
                        SsoId = empresa.Id,
                        IdBusinessUnit = empresa.IdBusinessUnit,
                        Nombre = empresa.Nombre,
                        Firma = empresa.Firma,
                        Correo = empresa.Correo
                    }));
                    await _empresaRepository.AddEmpresaRangeAsync(ssoEmpresas);
                }
                catch (Exception e)
                {
                    Console.Write(e.Message);
                }
            }
        }
    }
}
