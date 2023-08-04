using System.Linq;
using nordelta.cobra.webapi.Models;
using nordelta.cobra.webapi.Repositories.Contexts;
using nordelta.cobra.webapi.Repositories.Contracts;

namespace nordelta.cobra.webapi.Repositories
{
    public class CompanyRepository : ICompanyRepository
    {
        private readonly RelationalDbContext _context;

        public CompanyRepository(RelationalDbContext context)
        {
            _context = context;
        }
        public Company GetByCuit(string cuit)
        {
            return _context.Companies.FirstOrDefault(e => e.Cuit.Equals(cuit));
        }

        public Company GetByRazonSocial(string razonSocial)
        {
            return _context.Companies.FirstOrDefault(e => e.SocialReason.ToLower() == razonSocial.Trim().ToLower());
        }
    }
}
