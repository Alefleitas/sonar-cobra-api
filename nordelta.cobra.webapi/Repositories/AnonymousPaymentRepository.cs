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
    public class AnonymousPaymentRepository : IAnonymousPaymentRepository
    {

        private readonly RelationalDbContext _context;

        public AnonymousPaymentRepository(RelationalDbContext context)
        {
            _context = context;
        }

        public List<AnonymousPayment> GetPendingForMigration()
        {
            var anonymousPayments = _context.AnonymousPayment.ToList();
            return anonymousPayments.Where(x => !x.Migrated).ToList();
        }

        public List<AnonymousPayment> GetAllPending()
        {
            var pendingStatusPayments = _context.AnonymousPayment.ToList();
            return pendingStatusPayments.Where(x => x.Status == PaymentStatus.Pending).ToList();
        }

        public void UpdateAll(List<AnonymousPayment> externDebinList)
        {
            foreach (AnonymousPayment externDebin in externDebinList)
            {
                this._context.AnonymousPayment.Attach(externDebin);
                EntityEntry<AnonymousPayment> entry = _context.Entry(externDebin);
                entry.Property(e => e.Status).IsModified = true;
            }

            _context.SaveChanges();
        }

        public void Save(AnonymousPayment anonymousPayment)
        {
            _context.AnonymousPayment.Add(anonymousPayment);
            _context.SaveChanges();
        }

        public void CheckMigrated(int Id)
        {
            var anonymousPayment = _context.AnonymousPayment.SingleOrDefault(x => x.Id == Id);
            if (anonymousPayment != null)
            {
                anonymousPayment.Migrated = true;
                _context.SaveChanges();
            }
        }

        public async Task<AnonymousPayment> GetByDebinCode(string debinCode)
        {
            return await _context.AnonymousPayment.SingleOrDefaultAsync(x => x.DebinCode.ToLower() == debinCode.Trim().ToLower());
        }
    }
}
