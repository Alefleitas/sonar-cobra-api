using nordelta.cobra.webapi.Models;

namespace nordelta.cobra.webapi.Services.Contracts
{
    public interface ICompanyService
    {
        Company GetByRazonSocial(string razonSocial);

    }
}
