using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Query;
using nordelta.cobra.webapi.Models;
using nordelta.cobra.webapi.Repositories.Contexts;
using nordelta.cobra.webapi.Repositories.Contracts;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;

namespace nordelta.cobra.webapi.Repositories
{
    public class PaymentMethodRepository : IPaymentMethodRepository
    {
        private readonly RelationalDbContext _context;

        public PaymentMethodRepository(RelationalDbContext context)
        {
            _context = context;
        }

        public bool Insert(PaymentMethod paymentMethod)
        {
            switch (paymentMethod)
            {
                case Echeq echeq:
                    {
                        _context.Echeq.Add(echeq);
                        return _context.SaveChanges() > 0;
                    };
                case CvuOperation cvuOperation:
                    {
                        _context.CvuOperations.Add(cvuOperation);
                        return _context.SaveChanges() > 0;
                    };
                case Cash cash:
                    {
                        _context.Cash.Add(cash);
                        return _context.SaveChanges() > 0;
                    };
                case Cheque cheque:
                    {
                        _context.Cheque.Add(cheque);
                        return _context.SaveChanges() > 0;
                    };
            };

            return false;
        }

        public bool Update(PaymentMethod paymentMethod)
        {
            switch (paymentMethod)
            {
                case Echeq echeq:
                    {
                        _context.Echeq.Update(echeq);
                        return _context.SaveChanges() > 0;
                    };
                case CvuOperation cvuOperation:
                    {
                        _context.CvuOperations.Update(cvuOperation);
                        return _context.SaveChanges() > 0;
                    };
                case Cash cash:
                    {
                        _context.Cash.Update(cash);
                        return _context.SaveChanges() > 0;
                    };
                case Cheque cheque:
                    {
                        _context.Cheque.Update(cheque);
                        return _context.SaveChanges() > 0;
                    };
                case Debin debin:
                    {
                        _context.Debin.Update(debin);
                        return _context.SaveChanges() > 0;
                    }
            };

            return false;
        }

        public PaymentMethod GetSingle(
            Expression<Func<PaymentMethod, bool>> predicate = null,
            Func<IQueryable<PaymentMethod>, IOrderedQueryable<PaymentMethod>> orderBy = null,
            Func<IQueryable<PaymentMethod>, IIncludableQueryable<PaymentMethod, object>> include = null,
            bool noTracking = true)
        {
            IQueryable<PaymentMethod> query = _context.PaymentMethod;
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

        public List<PaymentMethod> GetAll(
            Expression<Func<PaymentMethod, bool>> predicate = null,
            Func<IQueryable<PaymentMethod>, IOrderedQueryable<PaymentMethod>> orderBy = null,
            Func<IQueryable<PaymentMethod>, IIncludableQueryable<PaymentMethod, object>> include = null,
            bool noTracking = true)
        {
            IQueryable<PaymentMethod> query = _context.PaymentMethod;
            if (noTracking)
            {
                query = query.AsNoTracking();
            }

            if (include != null)
            {
                query = include.Invoke(query);
            }

            if (predicate != null)
            {
                query = query.Where(predicate);
            }

            return orderBy != null
                ? orderBy(query).ToList()
                : query.ToList();
        }

        public Echeq GetSingleEcheq(
            Expression<Func<Echeq, bool>> predicate = null,
            Func<IQueryable<Echeq>, IOrderedQueryable<Echeq>> orderBy = null,
            Func<IQueryable<Echeq>, IIncludableQueryable<Echeq, object>> include = null,
            bool noTracking = true)
        {
            IQueryable<Echeq> query = _context.Echeq;
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

        public CvuOperation GetSingleCvuOperation(
            Expression<Func<CvuOperation, bool>> predicate = null,
            Func<IQueryable<CvuOperation>, IOrderedQueryable<CvuOperation>> orderBy = null,
            Func<IQueryable<CvuOperation>, IIncludableQueryable<CvuOperation, object>> include = null,
            bool noTracking = true)
        {
            IQueryable<CvuOperation> query = _context.CvuOperations;
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

        public Cash GetSingleCash(
            Expression<Func<Cash, bool>> predicate = null,
            Func<IQueryable<Cash>, IOrderedQueryable<Cash>> orderBy = null,
            Func<IQueryable<Cash>, IIncludableQueryable<Cash, object>> include = null,
            bool noTracking = true)
        {
            IQueryable<Cash> query = _context.Cash;
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

        public Cheque GetSingleCheque(
            Expression<Func<Cheque, bool>> predicate = null,
            Func<IQueryable<Cheque>, IOrderedQueryable<Cheque>> orderBy = null,
            Func<IQueryable<Cheque>, IIncludableQueryable<Cheque, object>> include = null,
            bool noTracking = true)
        {
            IQueryable<Cheque> query = _context.Cheque;
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



    }
}
