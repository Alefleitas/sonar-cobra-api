using nordelta.cobra.webapi.Services.Contracts;
using nordelta.cobra.webapi.Repositories.Contracts;
using System.Collections.Generic;
using nordelta.cobra.webapi.Models;
using System.Linq;
using System;
using System.Diagnostics;
using System.Threading.Tasks;
using Hangfire;
using Serilog;
using Newtonsoft.Json;
using nordelta.cobra.webapi.Services.DTOs;

namespace nordelta.cobra.webapi.Services
{
    public class UserService : IUserService
    {
        protected readonly IUserRepository _userRepository;
        protected readonly ILoginService _loginService;
        private readonly IContactDetailService _contactDetailService;
        private readonly IForeignCuitCacheRepository _foreignCuitCacheRepository;

        public UserService(
            IUserRepository userRepository, 
            ILoginService loginService,
            IContactDetailService contactDetailService,
            IForeignCuitCacheRepository foreignCuitCacheRepository
            )
        {
            _userRepository = userRepository;
            _loginService = loginService;
            _contactDetailService = contactDetailService;
            _foreignCuitCacheRepository = foreignCuitCacheRepository;
        }

        [Queue("sqlite")]
        [DisableConcurrentExecution(timeoutInSeconds: 1800)]
        public async Task SyncSsoUsers()
        {
            var ssoUsers = new List<SsoUser>();
            var contacts = new List<ContactDetailDto>();
            var usersResponse = _loginService.GetSsoUsers();

            if (usersResponse != null && usersResponse.Any())
            {
                try
                {
                    var cuits = usersResponse.Select(x => x.UserDataCuits).SelectMany(x => x).Distinct().ToList();
                    var limit = 1000.00; // 1000 es el limite que permite Oracle para consultar Contactos
                    var page = Math.Ceiling(cuits.Count / limit); // Indica la cantidad de veces que tiene que iterar

                    for (int i = 0; i < page; i++)
                    {
                        var tempCuits = cuits.GetRange(0, cuits.Count >= limit ? (int)limit : cuits.Count);
                        cuits.RemoveRange(0, cuits.Count >= limit ? (int)limit : cuits.Count);
                        contacts.AddRange(_contactDetailService.GetClienteDatosContactos(tempCuits)); 
                    }

                    ssoUsers.AddRange(usersResponse.Select(user => new SsoUser
                    {
                        IdApplicationUser = user.IdApplicationUser,
                        Cuit = user.Cuit,
                        Email = user.Email,
                        AccountNumber = user.NroCuenta,
                        IsForeignCuit = user.EsExtranjero,
                        ClientReference = user.ReferenciaCliente,
                        RazonSocial = user.RazonSocial,
                        TipoUsuario = user.TipoUsuario,
                        Roles = user.Roles.Select(x => new SsoUserRole {Role = x, UserId = user.IdApplicationUser})
                            .ToList(),
                        UserDataCuits = user.UserDataCuits
                            .Select(x => new SsoUserCuit {
                                Cuit = x, 
                                UserId = user.IdApplicationUser, 
                                RazonSocial = contacts.FirstOrDefault(y => y.DocumentNumber == x)?.PartyName 
                            }).ToList(),
                        Empresas = user.Empresas.Select(x => new SsoUserEmpresa()
                            {Empresa = x, UserId = user.IdApplicationUser}).ToList(),
                    }));

                    await _userRepository.AddUserRangeAsync(ssoUsers);
                }
                catch (Exception ex)
                {
                    Log.Error("Error sincronizando usuarios del SSO. Exception detail: {@ex}", ex);
                }
            }
        }

        public IEnumerable<string> GetEmailsForRole(string role)
        {
            var roles = new List<string>();
            try
            {
                roles = _userRepository.GetEmailsForRole(role).ToList();
                return roles;
            }
            catch (Exception ex)
            {
                Log.Error("Error obteniendo emails de usuarios. Exception detail: {@ex}", ex);
                return roles;
            }
        }

        [Queue("sqlite")]
        [DisableConcurrentExecution(timeoutInSeconds: 1800)]
        public async Task SyncForeignCuits()
        {
            try
            {
                var foreignCuits = await _loginService.GetAllForeignCuits();
                await _foreignCuitCacheRepository.AddAsync(foreignCuits);
            }
            catch (Exception ex)
            {
                Log.Error(ex, "Error sincronizando cuits extrajeros del SSO. Exception detail: {msg}", ex.Message); 
            }
        }
    }
}
