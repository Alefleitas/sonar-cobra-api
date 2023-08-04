using Microsoft.EntityFrameworkCore;
using nordelta.cobra.webapi.Models;
using nordelta.cobra.webapi.Repositories.Contexts;
using nordelta.cobra.webapi.Repositories.Contracts;
using Serilog;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using Hangfire;

namespace nordelta.cobra.webapi.Repositories
{
    public class UserRepository : IUserRepository
    {
        private readonly InMemoryDbContext _context;
        private readonly IRoleRepository _roleRepository;

        public UserRepository(InMemoryDbContext context, IRoleRepository roleRepository)
        {
            this._context = context;
            this._roleRepository = roleRepository;
        }
        
        [Queue("sqlite")]
        public void RemoveUsers()
        {
            if (HasUsers())
            {
                _context.Database.ExecuteSqlRaw("DELETE FROM SsoUserRoles;");
                _context.Database.ExecuteSqlRaw("DELETE FROM SsoUserCuits;");
                _context.Database.ExecuteSqlRaw("DELETE FROM SsoUserEmpresas;");
                _context.Database.ExecuteSqlRaw("DELETE FROM SsoUser;");
            }
        }
        
        
        public async Task AddUserRangeAsync(IEnumerable<SsoUser> ssoUsers)
        {
            Debug.WriteLine("Adding Users");
            await this._context.SsoUsers.AddRangeAsync(ssoUsers);
            await this._context.SaveChangesAsync();
        }

        public bool HasUsers()
        {
            return this._context.SsoUsers.Any();
        }

        //All Cobra users
        public List<SsoUser> GetAllUsers()
        {
            return this._context.SsoUsers
                .Include(x => x.Roles)
                .Include(x => x.Empresas)
                .Include(x => x.UserDataCuits)
                .ToList();
        }

        public SsoUser GetSsoUserByCuit(string cuit)
        {
            return this._context.SsoUsers
                .Include(x => x.Roles)
                .Include(x => x.UserDataCuits)
                .Where(x => x.Cuit == cuit || x.UserDataCuits.Any(y => y.Cuit == cuit))
                .FirstOrDefault();
        }

        public User GetUserById(string id)
        {
            var ssoUser = this._context.SsoUsers
                .Include(x => x.Roles)
                .Include(x => x.UserDataCuits)
                .SingleOrDefault(x => x.IdApplicationUser == id);

            if (ssoUser == null)
            {
                throw new ArgumentException("Error en GetUserById, no se encontro el usuario con id: {id}\", id");
            }

            return new User
            {
                Id = id,
                FirstName = ssoUser.RazonSocial,
                LastName = "",
                Email = ssoUser.Email,
                Cuit = long.Parse(ssoUser.Cuit),
                AdditionalCuits = ssoUser.UserDataCuits.Select(x => x.Cuit).ToList(),
                Roles = _roleRepository.get(ssoUser.Roles.Select(x => x.Role).ToList()),
                BirthDate = DateTime.Now,
            };
        }

        public SsoUser GetSsoUserById(string id) 
        {
            return this._context.SsoUsers
                .Include(x => x.Roles)
                .Include(x => x.UserDataCuits)
                .Where(x => x.IdApplicationUser == id).SingleOrDefault();
        }

        public List<SsoUser> GetUsersByCuits(List<string> cuits)
        {
            return this._context.SsoUsers
                .Include(x => x.Roles)
                .Include(x => x.UserDataCuits)
                .Where(x => cuits.Any(y => y == x.Cuit))
                .ToList();
        }

        public List<SsoUser> GetUsersByIds(List<string> ids)
        {
            return this._context.SsoUsers
                .Include(x => x.Roles)
                .Include(x => x.UserDataCuits)
                .Where(x => ids.Any(y => y == x.IdApplicationUser))
                .ToList();
        }

        public List<SsoUser> GetUsersByRoles(List<string> roles)
        {
            return this._context.SsoUsers
                .Include(x => x.Roles)
                .Include(x => x.UserDataCuits)
                .Where(x => x.Roles.Any(y => roles.Contains(y.Role)))
                .ToList();
        }

        public IEnumerable<string> GetEmailsForRole(string role)
        {
            return this._context.SsoUsers
                .Include(x => x.Roles)
                .Include(x => x.UserDataCuits)
                .Where(x => x.Roles.Any(y => y.Role.ToLower().Equals(role.ToLower())))
                .Select(x => x.Email)
                .ToList();
        }

        public List<string> FindUserIdsByName(string name)
        {
            throw new NotImplementedException();
        }

        public List<SsoUser> GetUsersDataByCuits(List<string> cuits)
        {
            throw new NotImplementedException();
        }

        public List<SsoUserCuit> FindUsersDataByName(string name)
        {
            throw new NotImplementedException();
        }

        public User GetUserByCuit(string cuit)
        {
            throw new NotImplementedException();
        }

        public List<SsoUser> FindUsersByName(string name)
        {
            throw new NotImplementedException();
        }
    }
}
