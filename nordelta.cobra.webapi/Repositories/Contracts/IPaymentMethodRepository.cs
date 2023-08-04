using Microsoft.EntityFrameworkCore.Query;
using nordelta.cobra.webapi.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;

namespace nordelta.cobra.webapi.Repositories.Contracts
{
    public interface IPaymentMethodRepository
    {
        bool Insert(PaymentMethod paymentMethod);
        bool Update(PaymentMethod paymentMethod);

        PaymentMethod GetSingle(
            Expression<Func<PaymentMethod, bool>> predicate = null,
            Func<IQueryable<PaymentMethod>, IOrderedQueryable<PaymentMethod>> orderBy = null,
            Func<IQueryable<PaymentMethod>, IIncludableQueryable<PaymentMethod, object>> include = null,
            bool noTracking = true);

        List<PaymentMethod> GetAll(
            Expression<Func<PaymentMethod, bool>> predicate = null,
            Func<IQueryable<PaymentMethod>, IOrderedQueryable<PaymentMethod>> orderBy = null,
            Func<IQueryable<PaymentMethod>, IIncludableQueryable<PaymentMethod, object>> include = null,
            bool noTracking = true);

        Echeq GetSingleEcheq(
            Expression<Func<Echeq, bool>> predicate = null,
            Func<IQueryable<Echeq>, IOrderedQueryable<Echeq>> orderBy = null,
            Func<IQueryable<Echeq>, IIncludableQueryable<Echeq, object>> include = null,
            bool noTracking = true);

        CvuOperation GetSingleCvuOperation(
            Expression<Func<CvuOperation, bool>> predicate = null,
            Func<IQueryable<CvuOperation>, IOrderedQueryable<CvuOperation>> orderBy = null,
            Func<IQueryable<CvuOperation>, IIncludableQueryable<CvuOperation, object>> include = null,
            bool noTracking = true);

        Cash GetSingleCash(
            Expression<Func<Cash, bool>> predicate = null,
            Func<IQueryable<Cash>, IOrderedQueryable<Cash>> orderBy = null,
            Func<IQueryable<Cash>, IIncludableQueryable<Cash, object>> include = null,
            bool noTracking = true);

        Cheque GetSingleCheque(
            Expression<Func<Cheque, bool>> predicate = null,
            Func<IQueryable<Cheque>, IOrderedQueryable<Cheque>> orderBy = null,
            Func<IQueryable<Cheque>, IIncludableQueryable<Cheque, object>> include = null,
            bool noTracking = true);
    }
}
