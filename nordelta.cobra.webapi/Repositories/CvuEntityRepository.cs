using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Query;
using nordelta.cobra.webapi.Models;
using nordelta.cobra.webapi.Repositories.Contexts;
using nordelta.cobra.webapi.Repositories.Contracts;

namespace nordelta.cobra.webapi.Repositories
{
    public class CvuEntityRepository : ICvuEntityRepository
    {
        private readonly RelationalDbContext _context;

        public CvuEntityRepository(RelationalDbContext context)
        {
            _context = context;
        }

        public CvuEntity GetById(int id)
        {
            return _context.CvuEntities.SingleOrDefault(it => it.Id == id);
        }

        public CvuEntity GetSingle(
            Expression<Func<CvuEntity, bool>> predicate = null,
            Func<IQueryable<CvuEntity>, IOrderedQueryable<CvuEntity>> orderBy = null,
            Func<IQueryable<CvuEntity>, IIncludableQueryable<CvuEntity, object>> include = null,
            bool noTracking = true)
        {
            IQueryable<CvuEntity> query = _context.CvuEntities;
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

        public IList<CvuEntity> GetAll(Expression<Func<CvuEntity, CvuEntity>> selector,
            Expression<Func<CvuEntity, bool>> predicate = null,
            Func<IQueryable<CvuEntity>, IOrderedQueryable<CvuEntity>> orderBy = null,
            Func<IQueryable<CvuEntity>, IIncludableQueryable<CvuEntity, object>> include = null,
            bool noTracking = true)
        {
            IQueryable<CvuEntity> query = _context.CvuEntities;
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
                ? orderBy(query).Select(selector).ToList()
                : query.Select(selector).ToList();
        }

        public bool Insert(CvuEntity entity)
        {
            _context.CvuEntities.Add(entity);
            return _context.SaveChanges() > 0;
        }

        public bool Update(CvuEntity entity)
        {
            _context.CvuEntities.Update(entity);
            return _context.SaveChanges() > 0;
        }

    }
}
