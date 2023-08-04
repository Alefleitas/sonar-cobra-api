using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using nordelta.cobra.webapi.Models;
using nordelta.cobra.webapi.Repositories.Contexts;
using nordelta.cobra.webapi.Repositories.Contracts;
using nordelta.cobra.webapi.Utils;
using Serilog;

namespace nordelta.cobra.webapi.Repositories
{
    public class BankAccountRepository : IBankAccountRepository
    {
        private readonly RelationalDbContext _context;
        private readonly IUserChangesLogRepository _userChangesLogRepository;

        public BankAccountRepository(RelationalDbContext context, IUserChangesLogRepository userChangesLogRepository)
        {
            _context = context;
            _userChangesLogRepository = userChangesLogRepository;
        }

        public List<BankAccount> All(string clientCuit, string accountNumber = "")
        {
            var query = _context.BankAccounts.Where(e => e.ClientCuit.Equals(clientCuit));
            if (!string.IsNullOrEmpty(accountNumber))
            {
                query = query.Where(x => x.ClientAccountNumber == accountNumber);
            }

            List<BankAccount> result = query.ToList();
            Log.Debug("Retriving BankAcccounts for cuit:{cuit} from db ({count})", clientCuit, result.Count);
            return result;
        }

        public List<BankAccount> All()
        {
            List<BankAccount> result = _context.BankAccounts.ToList();
            Log.Debug("Retriving all BankAcccounts from db ({count})", result.Count);
            return result;
        }

        public BankAccount GetByCbu(string cbu)
        {
            BankAccount result = _context.BankAccounts.AsNoTracking().FirstOrDefault(e => e.Cbu.Equals(cbu));
            return result;
        }

        public List<BankAccount> All(List<string> userAdditionalCuits)
        {
            List<BankAccount> result = _context.BankAccounts.Where(e => userAdditionalCuits.Contains(e.ClientCuit)).ToList();
            return result;
        }

        public BankAccount GetAccountForClient(string clientCuit, string cbu, Currency currency)
        {
            BankAccount result = _context.BankAccounts.FirstOrDefault(e => e.ClientCuit.Equals(clientCuit) && e.Cbu.Equals(cbu) && e.Currency.Equals(currency));
            return result;
        }

        public void Add(List<BankAccount> bankAccounts)
        {
            _context.AddRange(bankAccounts);
            _context.SaveChanges();
        }
        public void Add(BankAccount bankAccount, User user)
        {
            _context.Add(bankAccount);
            _context.SaveChanges();
            AddLog(new UserChangesLog
            {

                EntityId = bankAccount.Id,
                ModifiedEntity = CobraEntity.BankAccount,
                ModifiedField = nameof(bankAccount.Id),
                UserEmail = user.Email,
                UserId = user.Id,
                ModifyDate = LocalDateTime.GetDateTimeNow()
            }
            );

        }

        public bool Delete(int id, User user)
        {
            try
            {
                var bankAccount = _context.BankAccounts.SingleOrDefault(x => x.Id == id);

                if (bankAccount != null)
                {
                    var executionStrategy = _context.Database.CreateExecutionStrategy();
                    executionStrategy.Execute(() =>
                    {
                        using (var transaction = _userChangesLogRepository.UnitOfWork.BeginTransaction())
                        {
                            AddLog(new UserChangesLog
                            {
                                EntityId = bankAccount.Id,
                                ModifiedEntity = CobraEntity.BankAccount,
                                ModifiedField = nameof(bankAccount.Id),
                                UserEmail = user.Email,
                                UserId = user.Id,
                                ModifyDate = LocalDateTime.GetDateTimeNow()


                            });

                            _context.Remove(bankAccount);
                            _context.SaveChanges();

                            _userChangesLogRepository.UnitOfWork.Commit(transaction);
                        }
                    });
                    return true;
                }
                else
                {
                    Serilog.Log.Warning($"No se encontró un BankAccount con id {id}");
                    return false;
                }
            }
            catch (Exception ex)
            {
                Serilog.Log.Error(ex, "Ocurrió un error eliminando la BankAccount con Id {id}", id);
                return false;
            }
        }

        public BankAccount Get(int Id)
        {
            return _context.BankAccounts.SingleOrDefault(x => x.Id == Id);
        }


        private void AddLog(UserChangesLog user)
        {
            _userChangesLogRepository.Add(user);
        }
    }
}
