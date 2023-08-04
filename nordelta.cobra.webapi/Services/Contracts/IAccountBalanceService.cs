using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using nordelta.cobra.webapi.Models;
using nordelta.cobra.webapi.Services.DTOs;

namespace nordelta.cobra.webapi.Services.Contracts
{
    public interface IAccountBalanceService
    {
        Task UpdateAccountBalancesAsync();
        bool UpdateAccountBalance(AccountBalanceDTO accountBalanceDto, User user);
        List<AccountBalanceDetailDto> GetAccountBalanceDetail(AccountBalance accountBalance);
        List<AccountBalance> GetAllAccountBalances(User user, string search, string project, int? department, int? balance);
        AccountBalancePagination GetAllAccountBalances(User user, int limit, int page, string search, string project, int? department, int? balance);
        IEnumerable<string> GetAllAccountBalanceBU(bool isExternal);
        List<ClientProductsDto> GetClientProductsByCuits(List<string> cuits);
        Task SendRepeatedLegalEmail();
        List<DeudaMoraResponseDto> GetAllDeudaMoraByProduct(List<DeudaMoraRequestDto> model);
        List<AccountBalance> GetClientByProduct(string product);
        AccountBalance GetAccountBalanceByCuitAndProduct(string cuit, string codProducto);
        Task SyncRazonesSocialesAsync();
    }
}
