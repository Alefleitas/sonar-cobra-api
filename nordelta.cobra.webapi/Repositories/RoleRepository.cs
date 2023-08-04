using Microsoft.EntityFrameworkCore;
using nordelta.cobra.webapi.Models;
using nordelta.cobra.webapi.Repositories.Contexts;
using nordelta.cobra.webapi.Repositories.Contracts;
using System;
using System.Collections.Generic;
using System.Linq;

namespace nordelta.cobra.webapi.Repositories
{
    public class RoleRepository : IRoleRepository
    {
        private readonly RelationalDbContext _context;

        public RoleRepository(RelationalDbContext context)
        {
            _context = context;
        }
        public List<Role> get(List<string> rolesNames = null)
        {
            if (rolesNames == null) // All
            {
                return _context.Roles.Include(e => e.Permissions).ToList();
            }
            
            return _context.Roles.Where(x => rolesNames.Contains(x.Name)).Include(e => e.Permissions).ToList();
        }
    }
}
