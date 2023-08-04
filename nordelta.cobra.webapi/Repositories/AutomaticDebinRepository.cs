using Microsoft.EntityFrameworkCore;
using nordelta.cobra.webapi.Models;
using nordelta.cobra.webapi.Repositories.Contexts;
using nordelta.cobra.webapi.Repositories.Contracts;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore.ChangeTracking;
using Serilog;

namespace nordelta.cobra.webapi.Repositories
{
    public class AutomaticDebinRepository : IAutomaticDebinRepository
    {

        private readonly RelationalDbContext _context;

        public AutomaticDebinRepository(RelationalDbContext context)
        {
            _context = context;
        }

        public void Add(AutomaticPayment automaticPayment)
        {
            _context.AutomaticPayments.Add(automaticPayment);
            _context.SaveChanges();
        }

        public List<AutomaticPayment> All(User user)
        {
            var automaticDebits = _context.AutomaticPayments
                .Include(x => x.BankAccount)
                .ToList()
                .Where(x => x.Payer.Id == user.Id).ToList();
            Log.Debug("Returns {count} AutomaticPayments from db for user: {user}", automaticDebits.Count, user.Email);

            return automaticDebits;
        }

        public List<AutomaticPayment> All()
        {
            return _context.AutomaticPayments.Include(x => x.BankAccount).ToList();
        }

        public void Delete(int Id)
        {
            var automaticPayment = _context.AutomaticPayments.SingleOrDefault(x => x.Id == Id);
            if (automaticPayment != null)
            {
                _context.AutomaticPayments.Remove(automaticPayment);
                _context.SaveChanges();
            }
        }

        public AutomaticPayment Get(int Id)
        {
            return _context.AutomaticPayments.Include(x => x.BankAccount).SingleOrDefault(x => x.Id == Id);
        }
    }
}
