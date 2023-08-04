using nordelta.cobra.webapi.Models;
using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Threading.Tasks;

namespace nordelta.cobra.webapi.Repositories.Contracts;

public interface IPublishClientRepository
{
    Task<PublishClient> AddAsync(PublishClient entity);
    Task<PublishClient> UpdateAsync(PublishClient entity);
    Task<PublishClient> GetOneAsync(Expression<Func<PublishClient, bool>> predicate = null);
    Task<List<PublishClient>> GetAll();
}