using nordelta.cobra.webapi.Models;

namespace nordelta.cobra.webapi.Repositories.Contracts
{
    public interface ICompanyRepository
    {
        Company GetByCuit(string cuit);
        Company GetByRazonSocial(string razonSocial);
    }
}