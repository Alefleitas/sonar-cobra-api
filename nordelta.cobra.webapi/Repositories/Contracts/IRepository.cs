using System;
using System.Collections.Generic;
using nordelta.cobra.webapi.Models;

namespace nordelta.cobra.webapi.Repositories.Contracts
{
    public interface IRepository<T> where T : BaseEntity
    {
        IEnumerable<T> GetAll();
        T GetById(int id);
        int Insert(T entity);
        int Update(T entity);
        int Delete(int id);
    }
}
