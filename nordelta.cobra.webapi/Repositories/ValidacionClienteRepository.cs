using Microsoft.EntityFrameworkCore;
using nordelta.cobra.webapi.Models;
using nordelta.cobra.webapi.Repositories.Contexts;
using nordelta.cobra.webapi.Repositories.Contracts;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;

namespace nordelta.cobra.webapi.Repositories;

public class ValidacionClienteRepository : IValidacionClienteRepository
{
    private readonly RelationalDbContext _context;

    public ValidacionClienteRepository(RelationalDbContext relationalDbContext)
	{
        _context = relationalDbContext;
    }
    
    public IEnumerable<ValidacionCliente> Sync(IEnumerable<ValidacionCliente> entities)
    {
        var strategy = _context.Database.CreateExecutionStrategy();

        return strategy.Execute(() =>
            {
                using (var transaction = _context.Database.BeginTransaction())
                {

                    try
                    {
                        var oldEntities = _context.ValidacionCliente.AsNoTracking().ToList();

                        _context.RemoveRange(oldEntities);
                        _context.SaveChanges();

                        _context.AddRange(entities);
                        _context.SaveChanges();

                        transaction.Commit();
                        return entities;
                    }
                    catch (Exception)
                    {
                        transaction.Rollback();
                        throw;
                    }
                }
            });
    }

    public ValidacionCliente GetSingle(Expression<Func<ValidacionCliente, bool>> predicate)
    {
        return _context.ValidacionCliente.FirstOrDefault(predicate);
    }
}
