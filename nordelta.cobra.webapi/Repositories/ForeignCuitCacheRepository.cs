using Microsoft.Extensions.Caching.Distributed;
using Newtonsoft.Json;
using nordelta.cobra.webapi.Models;
using nordelta.cobra.webapi.Repositories.Contracts;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace nordelta.cobra.webapi.Repositories
{
    public class ForeignCuitCacheRepository : IForeignCuitCacheRepository
    {
        private readonly IDistributedCache _distributedCache;
        private readonly string _ssoForeignCuitKey = "ssoForeignCuit";

        public ForeignCuitCacheRepository(IDistributedCache distributedCache)
        {
            _distributedCache = distributedCache;
        }
        
        public async Task AddAsync(IEnumerable<ForeignCuit> foreignCuits)
        {
            // save to cache
            var serializeObject = JsonConvert.SerializeObject(foreignCuits);
            byte[] cachedForeignCuits = Encoding.UTF8.GetBytes(serializeObject);
            await _distributedCache.SetAsync(_ssoForeignCuitKey, cachedForeignCuits);
        }
        
        public List<ForeignCuit> GetAll()
        {
            var foreignCuits = new List<ForeignCuit>();
            var cachedForeignCuits = _distributedCache.Get(_ssoForeignCuitKey);
            if (cachedForeignCuits != null)
            {
                var bytesAsString = Encoding.UTF8.GetString(cachedForeignCuits);
                foreignCuits = JsonConvert.DeserializeObject<List<ForeignCuit>>(bytesAsString);
            }
            return foreignCuits;
        }
    }
}
