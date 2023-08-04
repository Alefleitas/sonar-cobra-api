using Microsoft.EntityFrameworkCore.Query;
using nordelta.cobra.webapi.Models;
using nordelta.cobra.webapi.Repositories.Contexts;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Linq;
using System;
using Microsoft.EntityFrameworkCore;
using nordelta.cobra.webapi.Repositories.Contracts;

namespace nordelta.cobra.webapi.Repositories
{
    public class PublishedDebtBankFileRepository : IPublishedDebtBankFileRepository
    {
        private readonly RelationalDbContext _context;

        public PublishedDebtBankFileRepository(RelationalDbContext context)
        {
            _context = context;
        }

        public PublishedDebtBankFile GetSingle(
            Expression<Func<PublishedDebtBankFile, bool>> predicate = null,
            Func<IQueryable<PublishedDebtBankFile>, IOrderedQueryable<PublishedDebtBankFile>> orderBy = null,
            Func<IQueryable<PublishedDebtBankFile>, IIncludableQueryable<PublishedDebtBankFile, object>> include = null,
            bool noTracking = true)
        {
            IQueryable<PublishedDebtBankFile> query = _context.PublishedDebtBankFile;
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

        public List<PublishedDebtBankFile> GetAll(
            Expression<Func<PublishedDebtBankFile, bool>> predicate = null,
            Func<IQueryable<PublishedDebtBankFile>, IOrderedQueryable<PublishedDebtBankFile>> orderBy = null,
            Func<IQueryable<PublishedDebtBankFile>, IIncludableQueryable<PublishedDebtBankFile, object>> include = null,
            bool noTracking = true)
        {
            IQueryable<PublishedDebtBankFile> query = _context.PublishedDebtBankFile;
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

        public bool Insert(PublishedDebtBankFile publishedDebtBankFile)
        {
            _context.PublishedDebtBankFile.Add(publishedDebtBankFile);
            return _context.SaveChanges() > 0;
        }

        public bool Update(PublishedDebtBankFile publishedDebtBankFile)
        {
            _context.PublishedDebtBankFile.Update(publishedDebtBankFile);
            return _context.SaveChanges() > 0;
        }
    }
}
