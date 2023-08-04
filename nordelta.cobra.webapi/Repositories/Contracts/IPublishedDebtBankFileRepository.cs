using Microsoft.EntityFrameworkCore.Query;
using nordelta.cobra.webapi.Models;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Linq;
using System;

namespace nordelta.cobra.webapi.Repositories.Contracts
{
    public interface IPublishedDebtBankFileRepository
    {
        PublishedDebtBankFile GetSingle(
            Expression<Func<PublishedDebtBankFile, bool>> predicate = null,
            Func<IQueryable<PublishedDebtBankFile>, IOrderedQueryable<PublishedDebtBankFile>> orderBy = null,
            Func<IQueryable<PublishedDebtBankFile>, IIncludableQueryable<PublishedDebtBankFile, object>> include = null,
            bool noTracking = true);

        List<PublishedDebtBankFile> GetAll(
            Expression<Func<PublishedDebtBankFile, bool>> predicate = null,
            Func<IQueryable<PublishedDebtBankFile>, IOrderedQueryable<PublishedDebtBankFile>> orderBy = null,
            Func<IQueryable<PublishedDebtBankFile>, IIncludableQueryable<PublishedDebtBankFile, object>> include = null,
            bool noTracking = true);

        bool Insert(PublishedDebtBankFile sentFile);

        bool Update(PublishedDebtBankFile sentFile);
    }
}
