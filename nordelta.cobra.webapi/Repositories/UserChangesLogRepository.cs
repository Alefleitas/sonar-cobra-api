using nordelta.cobra.webapi.Models;
using nordelta.cobra.webapi.Repositories.Contexts;
using nordelta.cobra.webapi.Repositories.Contracts;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace nordelta.cobra.webapi.Repositories
{
    public class UserChangesLogRepository : IUserChangesLogRepository
    {
        private readonly RelationalDbContext _context;
        public IUnitOfWork UnitOfWork => _context;

        public UserChangesLogRepository(RelationalDbContext context)
        {
            _context = context;
        }

        public void Add(UserChangesLog userChanges)
        {
            _context.UserChangesLog.Add(userChanges);
            _context.SaveChanges();
        }

        public void AddRange(List<UserChangesLog> userChangesLog)
        {
            _context.UserChangesLog.AddRange(userChangesLog);
            _context.SaveChanges();
        }
    }
}
