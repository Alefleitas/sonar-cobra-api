using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.EntityFrameworkCore;
using nordelta.cobra.webapi.Models;
using nordelta.cobra.webapi.Repositories.Contexts;
using nordelta.cobra.webapi.Repositories.Contracts;

namespace nordelta.cobra.webapi.Repositories
{
    public class Repository<T> : IRepository<T> where T : BaseEntity
    {
        protected readonly RelationalDbContext context;
        protected DbSet<T> entities;
        string errorMessage = string.Empty;
        public Repository(RelationalDbContext context)
        {
            this.context = context;
            entities = context.Set<T>();
        }
        public IEnumerable<T> GetAll()
        {
            return entities.ToList();
        }
        public T GetById(int id)
        {
            return entities.SingleOrDefault(s => s.Id == id);
        }
        public int Insert(T entity)
        {
            if (entity == null) throw new ArgumentNullException("entity");

            entities.Add(entity);
            return context.SaveChanges();
        }
        public int Update(T entity)
        {
            if (entity == null) throw new ArgumentNullException("entity");
            return context.SaveChanges();
        }
        public int Delete(int id)
        {
            T entity = entities.SingleOrDefault(s => s.Id == id);
            entities.Remove(entity ?? throw new InvalidOperationException());
            return context.SaveChanges();
        }
    }
}
