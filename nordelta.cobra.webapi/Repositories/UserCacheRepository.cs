using Microsoft.Extensions.Caching.Distributed;
using Newtonsoft.Json;
using nordelta.cobra.webapi.Models;
using nordelta.cobra.webapi.Repositories.Contracts;
using Serilog;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace nordelta.cobra.webapi.Repositories
{
    public class UserCacheRepository : IUserRepository
    {
        private readonly IDistributedCache _distributedCache;
        //cache key
        private readonly string _userListKey = "ssoUserList";
        private readonly IRoleRepository _roleRepository;

        private List<SsoUser> SsoUsers => this.GetAllUsers();
        public UserCacheRepository(IDistributedCache distributedCache, IRoleRepository roleRepository)
        {
            _distributedCache = distributedCache;
            this._roleRepository = roleRepository;
        }

        public async Task AddUserRangeAsync(IEnumerable<SsoUser> ssoUsers)
        {
            // save to cache
            var serializeObject = JsonConvert.SerializeObject(ssoUsers);
            byte[] cachedUsers = Encoding.UTF8.GetBytes(serializeObject);
            await _distributedCache.SetAsync(_userListKey, cachedUsers);
        }

        public List<SsoUser> GetAllUsers()
        {
            List<SsoUser> ssoUsers = new List<SsoUser>();
            var cachedUsers = _distributedCache.Get(_userListKey);
            if (cachedUsers != null)
            {
                var bytesAsString = Encoding.UTF8.GetString(cachedUsers);
                ssoUsers = JsonConvert.DeserializeObject<List<SsoUser>>(bytesAsString);
            }
            return ssoUsers;
        }

        public IEnumerable<string> GetEmailsForRole(string role)
        {
            return this.SsoUsers
               .Where(x => x.Roles.Any(y => y.Role.ToLower().Equals(role.ToLower())))
               .Select(x => x.Email)
               .ToList();
        }

        public SsoUser GetSsoUserById(string id)
        {
            return this.SsoUsers.Where(x => x.IdApplicationUser == id).SingleOrDefault();
        }

        public SsoUser GetSsoUserByCuit(string cuit)
        {
            return this.SsoUsers.Where(x => x.Cuit == cuit || x.UserDataCuits.Any(y => y.Cuit == cuit))
                .FirstOrDefault();
        }
        
        public User GetUserByCuit(string cuit)
        {
            var ssoUser = this.SsoUsers.FirstOrDefault(x => x.Cuit == cuit || x.UserDataCuits.Any(y => y.Cuit == cuit));

            if (ssoUser is null)
            {
                Log.Error("Error en GetUserByCuit, no se encontro el usuario sso con Cuit: {cuit}", cuit);
                return null;
            }

            return new User
            {
                Id = ssoUser.IdApplicationUser,
                FirstName = ssoUser.RazonSocial,
                LastName = "",
                Email = ssoUser.Email,
                Cuit = long.Parse(ssoUser.Cuit),
                AdditionalCuits = ssoUser.UserDataCuits.Select(x => x.Cuit).ToList(),
                Roles = _roleRepository.get(ssoUser.Roles.Select(x => x.Role).ToList()),
                BirthDate = DateTime.Now,
            };
        }

        public List<string> FindUserIdsByName(string name)
        {
            var result = new List<string>();
            var users = this.SsoUsers.Where(x => x.RazonSocial.ToLower().Contains(name.ToLower())).ToList();
            if (users.Any())
            {
                result = users.Select(x => x.IdApplicationUser).ToList();
            }

            return result;
        }

        public List<SsoUser> FindUsersByName(string name)
        {
            var result = new List<SsoUser>();

            if (string.IsNullOrEmpty(name))
                return result;

            var usersData = this.SsoUsers.Where(x => !string.IsNullOrEmpty(x.RazonSocial) && x.RazonSocial.ToLower().Contains(name.ToLower()));

            if (usersData.Any())
            {
                result = usersData.ToList();
            }

            return result;
        }

        public List<SsoUserCuit> FindUsersDataByName(string name)
        {
            var result = new List<SsoUserCuit>();

            if (string.IsNullOrEmpty(name))
                return result;

            var usersData = this.SsoUsers.Select(x => x.UserDataCuits).SelectMany(x => x).Where(x => !string.IsNullOrEmpty(x.RazonSocial) && x.RazonSocial.ToLower().Contains(name.ToLower()));
            
            if (usersData.Any())
            {
                result = usersData.ToList();
            }

            return result;
        }

        public User GetUserById(string id)
        {
            var ssoUser = this.SsoUsers
                .SingleOrDefault(x => x.IdApplicationUser == id);

            if (ssoUser == null)
            {
                Log.Error("Error en GetUserById, no se encontro el usuario con id: {id}", id);
                return default;
            }

            return new User
            {
                Id = id,
                FirstName = ssoUser.RazonSocial,
                LastName = "",
                AccountNumber = ssoUser.AccountNumber,
                ClientReference = ssoUser.ClientReference,
                IsForeignCuit = ssoUser.IsForeignCuit,
                Email = ssoUser.Email,
                Cuit = long.Parse(ssoUser.Cuit),
                AdditionalCuits = ssoUser.UserDataCuits.Select(x => x.Cuit).ToList(),
                Roles = _roleRepository.get(ssoUser.Roles.Select(x => x.Role).ToList()),
                BirthDate = DateTime.Now,
            };
        }

        public List<SsoUser> GetUsersByCuits(List<string> cuits)
        {
            return this.SsoUsers
                .Where(x => cuits.Any(y => y == x.Cuit))
                .ToList();
        }
        
        public List<SsoUser> GetUsersDataByCuits(List<string> cuits)
        {
            return this.SsoUsers
               .Where(x => cuits.Contains(x.Cuit) || x.UserDataCuits.Any(x => cuits.Contains(x.Cuit)))
               .ToList();
        }

        public List<SsoUser> GetUsersByIds(List<string> ids)
        {
            return this.SsoUsers
                .Where(x => ids.Any(y => y == x.IdApplicationUser))
                .ToList();
        }

        public List<SsoUser> GetUsersByRoles(List<string> roles)
        {
            return this.SsoUsers
                .Where(x => x.Roles.Any(y => roles.Contains(y.Role)))
                .ToList();
        }

        public bool HasUsers()
        {
            return this.SsoUsers.Any();
        }

        public void RemoveUsers()
        {
            throw new NotImplementedException();
        }
    }
}
