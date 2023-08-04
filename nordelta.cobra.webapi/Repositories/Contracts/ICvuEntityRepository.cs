using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using Microsoft.EntityFrameworkCore.Query;
using nordelta.cobra.webapi.Models;

namespace nordelta.cobra.webapi.Repositories.Contracts
{
    public interface ICvuEntityRepository
    {
        CvuEntity GetById(int id);

        CvuEntity GetSingle(
            Expression<Func<CvuEntity, bool>> predicate = null,
            Func<IQueryable<CvuEntity>, IOrderedQueryable<CvuEntity>> orderBy = null,
            Func<IQueryable<CvuEntity>, IIncludableQueryable<CvuEntity, object>> include = null,
            bool noTracking = true);
        IList<CvuEntity> GetAll(
            Expression<Func<CvuEntity, CvuEntity>> selector,
            Expression<Func<CvuEntity, bool>> predicate = null,
            Func<IQueryable<CvuEntity>, IOrderedQueryable<CvuEntity>> orderBy = null,
            Func<IQueryable<CvuEntity>, IIncludableQueryable<CvuEntity, object>> include = null,
            bool noTracking = true);

        bool Insert(CvuEntity entity);
        bool Update(CvuEntity entity);

    }
}
