using Microsoft.EntityFrameworkCore;
using nordelta.cobra.webapi.Models;
using nordelta.cobra.webapi.Repositories.Contexts;
using nordelta.cobra.webapi.Repositories.Contracts;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore.ChangeTracking;

namespace nordelta.cobra.webapi.Repositories
{
    public class DebinRepository : IDebinRepository
    {

        private readonly RelationalDbContext _context;

        public DebinRepository(RelationalDbContext context)
        {
            _context = context;
        }
        public void UpdateAll(List<Debin> debinList)
        {
            foreach (Debin debin in debinList)
            {
                this._context.Debin.Attach(debin);
                EntityEntry<Debin> entry = _context.Entry(debin);
                entry.Property(e => e.Status).IsModified = true;
            }
            
            _context.SaveChanges();
        }

        public List<Debin> GetAllPayed(string clientCuit)
        {
            return _context.Debin
                .Where(debin => debin.Status == PaymentStatus.Approved && debin.BankAccount.ClientCuit == clientCuit)
                .Include(x => x.Debts)
                .Include(x => x.BankAccount)
                .ToList();
        }

        public List<Debin> GetAllPending()
        {
            return _context.Debin
                .Where(debin => debin.Status == PaymentStatus.Pending)
                .Include(x => x.BankAccount)
                .Include(x => x.Debts)
                .ToList();
        }

        public void Save(Debin debin)
        {
            _context.Debin.Add(debin);
            _context.SaveChanges();
        }

        public List<Debin> GetAllByIds(List<int> ids)
        {
            return _context.Debin.Where(x => ids.Contains(x.Id))
                .ToList();
        }

        public List<Debin> GetAll()
        {
            return _context.Debin.ToList();
        }
    }
}
