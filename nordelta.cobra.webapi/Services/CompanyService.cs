using nordelta.cobra.webapi.Models;
using nordelta.cobra.webapi.Repositories.Contracts;
using nordelta.cobra.webapi.Services.Contracts;

namespace nordelta.cobra.webapi.Services
{
    public class CompanyService : ICompanyService
    {
        private readonly ICompanyRepository _companyRepository;

        public CompanyService(ICompanyRepository companyRepositor)
        {
            _companyRepository = companyRepositor;
        }

        public Company GetByRazonSocial(string razonSocial)
        {
            return _companyRepository.GetByRazonSocial(razonSocial);
        }

    }
}
