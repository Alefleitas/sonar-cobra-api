using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using Microsoft.EntityFrameworkCore.Query;
using nordelta.cobra.webapi.Models;

namespace nordelta.cobra.webapi.Repositories.Contracts
{
    public interface IPaymentReportRepository
    {

        PaymentReport GetSingle(
            Expression<Func<PaymentReport, bool>> predicate = null,
            Func<IQueryable<PaymentReport>, IOrderedQueryable<PaymentReport>> orderBy = null,
            Func<IQueryable<PaymentReport>, IIncludableQueryable<PaymentReport, object>> include = null,
            bool noTracking = true);

        List<PaymentReport> GetAll(
            Expression<Func<PaymentReport, bool>> predicate = null,
            Func<IQueryable<PaymentReport>, IOrderedQueryable<PaymentReport>> orderBy = null,
            Func<IQueryable<PaymentReport>, IIncludableQueryable<PaymentReport, object>> include = null,
            bool noTracking = true);
        bool Insert(PaymentReport report);
        bool Update(PaymentReport report);
        bool UpdateAll(List<PaymentReport> reports);
        bool Save();

    }
}
