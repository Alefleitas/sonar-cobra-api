using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Dynamic.Core;
using System.Linq.Expressions;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Query;
using nordelta.cobra.webapi.Models;
using nordelta.cobra.webapi.Repositories.Contexts;
using nordelta.cobra.webapi.Repositories.Contracts;

namespace nordelta.cobra.webapi.Repositories
{
    public class PaymentReportsRepository : IPaymentReportRepository
    {
        private readonly RelationalDbContext _context;

        public PaymentReportsRepository(RelationalDbContext context)
        {
            _context = context;
        }

        public PaymentReport GetSingle(
            Expression<Func<PaymentReport, bool>> predicate = null,
            Func<IQueryable<PaymentReport>, IOrderedQueryable<PaymentReport>> orderBy = null,
            Func<IQueryable<PaymentReport>, IIncludableQueryable<PaymentReport, object>> include = null,
            bool noTracking = true)
        {
            IQueryable<PaymentReport> query = _context.PaymentReports;
            if (noTracking)
            {
                query = query.AsNoTracking();
            }

            if (include != null)
            {
                query = include(query);
            }

            if (predicate != null)
            {
                query = query.Where(predicate);
            }

            return orderBy != null
                ? orderBy(query).FirstOrDefault()
                : query.FirstOrDefault();
        }

        public List<PaymentReport> GetAll(
            Expression<Func<PaymentReport, bool>> predicate = null,
            Func<IQueryable<PaymentReport>, IOrderedQueryable<PaymentReport>> orderBy = null,
            Func<IQueryable<PaymentReport>, IIncludableQueryable<PaymentReport, object>> include = null,
            bool noTracking = true)
        {
            IQueryable<PaymentReport> query = _context.PaymentReports;
            if (noTracking)
            {
                query = query.AsNoTracking();
            }

            if (include != null)
            {
                query = include(query);
            }

            if (predicate != null)
            {
                query = query.Where(predicate);
            }

            return orderBy != null
                ? orderBy(query).ToList()
                : query.ToList();
        }

        public bool Insert(PaymentReport report)
        {
            _context.PaymentReports.Add(report);
            return _context.SaveChanges() > 0;
        }

        public bool Update(PaymentReport report)
        {
            _context.PaymentReports.Update(report);
            return _context.SaveChanges() > 0;
        }

        public bool UpdateAll(List<PaymentReport> reports)
        {
            foreach (var item in reports)
            {
                _context.PaymentReports.Update(item);
            }
            return _context.SaveChanges() > 0;

        }
        public bool Save()
        {
            return _context.SaveChanges() > 0;
        }
    }
}
