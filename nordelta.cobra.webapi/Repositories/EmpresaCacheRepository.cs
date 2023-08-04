using Microsoft.Extensions.Caching.Distributed;
using Newtonsoft.Json;
using nordelta.cobra.webapi.Models;
using nordelta.cobra.webapi.Repositories.Contracts;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace nordelta.cobra.webapi.Repositories
{
    public class EmpresaCacheRepository : IEmpresaRepository
    {
        private readonly IDistributedCache _distributedCache;
        //cache key
        private readonly string _empresaListKey = "ssoEmpresaList";

        private List<SsoEmpresa> SsoEmpresas => this.GetAllEmpresas();
        public EmpresaCacheRepository(IDistributedCache distributedCache)
        {
            _distributedCache = distributedCache;
        }

        public async Task AddEmpresaRangeAsync(IEnumerable<SsoEmpresa> ssoEmpresas)
        {
            // save to cache
            var serializeObject = JsonConvert.SerializeObject(ssoEmpresas);
            byte[] cachedEmpresas = Encoding.UTF8.GetBytes(serializeObject);
            await _distributedCache.SetAsync(_empresaListKey, cachedEmpresas);
        }

        public List<SsoEmpresa> GetAllEmpresas()
        {
            List<SsoEmpresa> ssoEmpresas = new List<SsoEmpresa>();
            var cachedEmpresas = _distributedCache.Get(_empresaListKey);
            if (cachedEmpresas != null)
            {
                var bytesAsString = Encoding.UTF8.GetString(cachedEmpresas);
                ssoEmpresas = JsonConvert.DeserializeObject<List<SsoEmpresa>>(bytesAsString);
            }
            return ssoEmpresas;
        }

        public SsoEmpresa GetByName(string name)
        {
            return this.SsoEmpresas.Where(x => x.Nombre.Trim().ToLower() == name.Trim().ToLower()).SingleOrDefault();
        }

        public bool HasEmpresas()
        {
            return this.SsoEmpresas.Any();
        }

        public void RemoveEmpresas()
        {
            throw new NotImplementedException();
        }
    }
}
