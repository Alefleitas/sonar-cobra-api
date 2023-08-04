using nordelta.cobra.webapi.Models;
using nordelta.cobra.webapi.Repositories.Contexts;
using nordelta.cobra.webapi.Repositories.Contracts;
using Serilog;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using Hangfire;
using Microsoft.EntityFrameworkCore;

namespace nordelta.cobra.webapi.Repositories
{
    public class EmpresaRepository : IEmpresaRepository
    {

        private readonly InMemoryDbContext _context;
        public EmpresaRepository(InMemoryDbContext _context)
        {
            this._context = _context;
        }

        [Queue("sqlite")]
        public void RemoveEmpresas()
        {
            if (HasEmpresas())
            {
                _context.Database.ExecuteSqlRaw("DELETE FROM SsoEmpresa;");
            }
        }

        [Queue("sqlite")]
        public async Task AddEmpresaRangeAsync(IEnumerable<SsoEmpresa> ssoEmpresas)
        {
            Debug.WriteLine("Adding Empresas");
            await this._context.SsoEmpresas.AddRangeAsync(ssoEmpresas);
            await this._context.SaveChangesAsync();
        }

        public bool HasEmpresas()
        {
            return this._context.SsoEmpresas.Any();
        }

        public SsoEmpresa GetByName(string name)
        {
            return this._context.SsoEmpresas.Where(x => x.Nombre.Trim().ToLower() == name.Trim().ToLower()).SingleOrDefault();
        }

        public List<SsoEmpresa> GetAllEmpresas()
        {
            throw new NotImplementedException();
        }
    }
}
