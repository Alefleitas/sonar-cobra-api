using System.Collections.Generic;
using nordelta.cobra.webapi.Models;

namespace nordelta.cobra.webapi.Repositories.Contracts
{
    public interface IBankAccountRepository
    {
        List<BankAccount> All();
        List<BankAccount> All(string clientCuit, string accountNumber);
        void Add(List<BankAccount> bankAccounts);
        void Add(BankAccount bankAccount, User user);
        BankAccount GetByCbu(string cbu);
        List<BankAccount> All(List<string> userAdditionalCuits);
        BankAccount GetAccountForClient(string clientCuit, string cbu, Currency currency);
        BankAccount Get(int Id);
        bool Delete(int id, User user);
    }
}