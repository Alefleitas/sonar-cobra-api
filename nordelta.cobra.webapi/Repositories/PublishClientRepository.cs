using Microsoft.EntityFrameworkCore;
using nordelta.cobra.webapi.Models;
using nordelta.cobra.webapi.Repositories.Contexts;
using nordelta.cobra.webapi.Repositories.Contracts;
using nordelta.cobra.webapi.Utils;
using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Threading.Tasks;

namespace nordelta.cobra.webapi.Repositories;

public class PublishClientRepository : IPublishClientRepository
{
    private readonly RelationalDbContext _context;

    public PublishClientRepository(RelationalDbContext context)
    {
        _context = context;
    }

    public async Task<PublishClient> AddAsync(PublishClient entity)
    {
        entity.CreatedOn = LocalDateTime.GetDateTimeNow();
        await _context.AddAsync(entity);
        await _context.SaveChangesAsync();
        return entity;
    }

    public async Task<PublishClient> UpdateAsync(PublishClient entity)
    {
        entity.LastUpdatedOn = LocalDateTime.GetDateTimeNow();
        _context.Update(entity);
        await _context.SaveChangesAsync();
        return entity;
    }

    public async Task<PublishClient> GetOneAsync(Expression<Func<PublishClient, bool>> predicate = null)
    {
        var entity = predicate is null ? await _context.PublishClient.FirstOrDefaultAsync() :
            await _context.PublishClient.FirstOrDefaultAsync(predicate);

        return entity;
    }

    public async Task<List<PublishClient>> GetAll()
    {
        return await _context.PublishClient.ToListAsync();
    }
}