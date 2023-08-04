using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Threading.Tasks;
using nordelta.cobra.webapi.Models;
using nordelta.cobra.webapi.Services.DTOs;

namespace nordelta.cobra.webapi.Repositories.Contracts
{
    public interface IAccountBalanceRepository
    {
        IUnitOfWork UnitOfWork { get; }
        AccountBalance GetAccountBalance(User user, string product);
        AccountBalance GetAccountBalance(string clientCuit, string product);
        List<AccountBalance> GetAllAccountBalances(User user, string search, string project, int? department, int? balance);
        AccountBalance GetAccountBalanceById(int accountBalanceId);
        void AddToLogAccountBalance(AccountBalanceDTO accountBalanceDto,AccountBalance accountBalanceInDb, User user);
        bool UpdateStatus(int accountBalanceId, AccountBalance.EContactStatus status);
        bool InsertOrUpdate(AccountBalance accountBalance);
        List<AccountBalance> InsertOrUpdate(List<AccountBalance> accounts);
        void CheckLegales(AccountBalance.EDepartment targetDepartment, int accountBalanceId);
        Task<List<DepartmentChangeNotification>> GetAllLegalesNotificationAsync();
        void CleanLegalesNotification();
        AccountBalance GetAccountBalanceByProductCuit(string product, string cuit);
        List<AccountBalance> GetAccountBalanceByCuits(List<string> cuits);
        List<AccountBalance> GetAccountBalanceByProduct(string product);
        IEnumerable<string> GetAllBU(bool isExternal);
        List<AccountBalance> GelAllAccountBalanceWithOutCVU();

        IEnumerable<string> GetPropertyCodesByCuits(IEnumerable<string> cuits);
        List<AccountBalance> GetAllByBusinessUnit(string buName);
        List<AccountBalance> GetAll(Expression<Func<AccountBalance, bool>> predicate);
        Task UpdateAll(List<AccountBalance> accountBalances);
    }
}
