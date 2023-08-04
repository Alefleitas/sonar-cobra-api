using System.Collections.Generic;
using nordelta.cobra.webapi.Models;
using nordelta.cobra.webapi.Services.DTOs;

namespace nordelta.cobra.webapi.Services.Contracts
{
    public interface IBankAccountService
    {
        List<BankAccount> GetBankAccountsForClient(string clientCuit, string accountNumber = "");
        List<BankAccount> GetBankAccountsForClient(List<string> userAdditionalCuits);
        BankAccount AddBankAccount(string clientCuit, string cbu, string cuit, Currency currency, User user);
        ValidateBankAccountResponse ValidateBankAccount(string cbu, string cuit);
        BankAccount GetBankAccountFromCbu(string clientCuit, string cbu, Currency currency, string accountNumber = "");
        bool DeleteBankAccount(int id, User user);
    }
}