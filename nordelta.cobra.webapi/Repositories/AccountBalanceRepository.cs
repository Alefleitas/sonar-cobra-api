using nordelta.cobra.webapi.Models;
using nordelta.cobra.webapi.Repositories.Contracts;
using System;
using System.Collections.Generic;
using System.Linq;
using nordelta.cobra.webapi.Repositories.Contexts;
using Microsoft.EntityFrameworkCore;
using Serilog;
using System.Threading.Tasks;
using nordelta.cobra.webapi.Controllers.Helpers;
using nordelta.cobra.webapi.Services.Contracts;
using Microsoft.Extensions.Options;
using nordelta.cobra.webapi.Configuration;
using Microsoft.Extensions.DependencyInjection;
using nordelta.cobra.webapi.Services.DTOs;
using nordelta.cobra.webapi.Utils;
using System.Linq.Expressions;

namespace nordelta.cobra.webapi.Repositories
{
    public class AccountBalanceRepository : IAccountBalanceRepository
    {
        private readonly RelationalDbContext _context;
        private readonly IUserRepository _userRepository;
        private readonly IUserChangesLogRepository _userChangesLogRepository;
        private readonly IServiceProvider _serviceProvider;
        private readonly CustomItauCvuConfiguration _itauCvuConfig;

        public IUnitOfWork UnitOfWork => _context;

        public AccountBalanceRepository(
            RelationalDbContext context, 
            IUserRepository userRepository, 
            IUserChangesLogRepository userChangesLogRepository,
            IServiceProvider serviceProvider, 
            IOptionsMonitor<CustomItauCvuConfiguration> customItauCvuConfig)
        {
            _context = context;
            _userRepository = userRepository;
            _userChangesLogRepository = userChangesLogRepository;
            _itauCvuConfig = customItauCvuConfig.Get(CustomItauCvuConfiguration.CustomItauCvuConfig);
            _serviceProvider = serviceProvider;
        }

        public List<AccountBalance> GetAllAccountBalances(User user, string search, string project, int? department, int? balance)
        {
            var listAccountBalance = _context.AccountBalances.ToList();
            listAccountBalance = AuthFilterBUHelper<AccountBalance>.FilterByBU(listAccountBalance, user);
            listAccountBalance = AuthFilterDepartmentHelper<AccountBalance>.FilterOnlyByDepartmentExterno(listAccountBalance, user);

            IQueryable<AccountBalance> queryable = listAccountBalance.AsQueryable();

            if (string.IsNullOrEmpty(search) && string.IsNullOrEmpty(project) && !department.HasValue && !balance.HasValue)
            {
                return queryable.Include(x => x.Communications)
                                .ThenInclude(y => y.ContactDetail).ToList();
            }

            if (!string.IsNullOrWhiteSpace(search))
            {
                queryable = queryable.Where(m => m.Product.Contains(search) || (m.RazonSocial != null && m.RazonSocial.ToUpper().Trim().Contains(search.ToUpper().Trim())));
            }
            if (!string.IsNullOrWhiteSpace(project))
            {
                queryable = queryable.Where(m => m.BusinessUnit.ToLower().Equals(project.ToLower()));
            }
            if (department.HasValue)
            {
                queryable = queryable.Where(m => m.Department == (AccountBalance.EDepartment)department.Value);
            }
            if (balance.HasValue)
            {
                queryable = queryable.Where(m => m.Balance == (AccountBalance.EBalance)balance.Value);
            }

            return queryable.Include(x => x.Communications)
                            .ThenInclude(y => y.ContactDetail).ToList();
        }

        public AccountBalance GetAccountBalance(User user, string product)
        {
            return _context.AccountBalances
                .Include(x => x.Communications)
                .ThenInclude(y => y.ContactDetail)
                .AsNoTracking()
                .FirstOrDefault(x => x.ClientId.Equals(user.Id) && x.Product.Equals(product));
        }

        public AccountBalance GetAccountBalance(string clientCuit, string product)
        {
            return _context.AccountBalances
                .Include(x => x.Communications)
                .ThenInclude(y => y.ContactDetail)
                .AsNoTracking()
                .FirstOrDefault(x => x.ClientCuit == clientCuit && x.Product.Contains(product.Trim()));
        }

        public AccountBalance GetAccountBalanceById(int accountBalanceId)
        {
            return _context.AccountBalances
                .Include(x => x.Communications)
                .ThenInclude(y => y.ContactDetail)
                .AsNoTracking()
                .SingleOrDefault(x => x.Id == accountBalanceId);
        }

        public void AddToLogAccountBalance(AccountBalanceDTO accountBalanceDto, AccountBalance accountBalanceInDb, User user)
        {
            UnitOfWork.RunWithExecutionStrategy(() =>
            {
                if (accountBalanceDto.Department != accountBalanceInDb.Department)
                {
                    AddLog(accountBalanceDto.Id, AcountBalanceEntity.Department, user);

                }

                if (accountBalanceDto.DelayStatus != accountBalanceInDb.DelayStatus)
                {
                    AddLog(accountBalanceDto.Id, AcountBalanceEntity.DelayStatus, user);
                }

                if (accountBalanceDto.WorkStarted != accountBalanceInDb.WorkStarted)
                {
                    AddLog(accountBalanceDto.Id, AcountBalanceEntity.WorkStarted, user);
                }

                if (!string.IsNullOrEmpty(accountBalanceDto.PublishDebt) && accountBalanceDto.PublishDebt != accountBalanceInDb.PublishDebt)
                {
                    AddLog(accountBalanceDto.Id, AcountBalanceEntity.PublishDebt, user);
                }
            });
        }

        public AccountBalance GetAccountBalanceByProductCuit(string product, string cuit)
        {
            return _context.AccountBalances
                .AsNoTracking()
                .SingleOrDefault(x => x.Product.ToLower() == product.ToLower() && x.ClientCuit == cuit);
        }

        public List<AccountBalance> GetAccountBalanceByProduct(string product)
        {
            var result = new List<AccountBalance>();
            if (!string.IsNullOrEmpty(product))
            {
                result = _context.AccountBalances
                    .AsNoTracking()
                    .Where(x => x.Product.ToLower() == product.ToLower())
                    .ToList();
            }
            return result;
        }

        public List<AccountBalance> GetAccountBalanceByCuits(List<string> cuits)
        {
            return _context.AccountBalances
                .AsNoTracking()
                .Where(x => cuits.Contains(x.ClientCuit))
                .ToList();
        }

        public bool InsertOrUpdate(AccountBalance accountBalance)
        {
            if (accountBalance.Id == 0)
            {
                _context.Add(accountBalance);
            }
            else
            {
                _context.Entry(accountBalance).State = EntityState.Modified;
                _context.Update(accountBalance);
            }

            return _context.SaveChanges() > 0;
        }

        public void CheckLegales(AccountBalance.EDepartment targetDepartment, int accountBalanceId)
        {
            var account = GetAccountBalanceById(accountBalanceId);
            if (targetDepartment == AccountBalance.EDepartment.Legales &&
                account.Department == AccountBalance.EDepartment.CuentasACobrar)
            {
                var user = _userRepository.GetSsoUserById(account.ClientId);

                var addLegalesModel = new DepartmentChangeNotification();
                addLegalesModel.CodigoProducto = account.Product;
                addLegalesModel.RazonSocial = user.RazonSocial;
                addLegalesModel.NumeroCuitCliente = account.ClientCuit;

                _context.DepartmentChangeNotifications.Add(addLegalesModel);
                _context.SaveChanges();
            }
        }

        public async Task<List<DepartmentChangeNotification>> GetAllLegalesNotificationAsync()
        {
            return await _context.DepartmentChangeNotifications.ToListAsync();
        }

        public bool UpdateStatus(int accountBalanceId, AccountBalance.EContactStatus status)
        {
            var accountBalance = _context.AccountBalances
                .SingleOrDefault(x => x.Id == accountBalanceId);
            if (accountBalance == null)
            {
                return false;
            }
            accountBalance.ContactStatus = status;
            _context.Update(accountBalance);

            return _context.SaveChanges() > 0;
        }

        public List<AccountBalance> InsertOrUpdate(List<AccountBalance> accounts)
        {
            var ret = new List<AccountBalance>();
            try
            {
                List<AccountBalance> toBeUpdated = new List<AccountBalance>();
                List<AccountBalance> toBeAdded = new List<AccountBalance>();
                foreach (var ab in accounts)
                {
                    var dbAccount = _context.AccountBalances.FirstOrDefault(x => x.ClientCuit == ab.ClientCuit && x.Product == ab.Product && (ab.ClientReference == null || x.ClientReference.Equals(ab.ClientReference)));
                    if (dbAccount != null)
                    {
                        toBeUpdated.Add(ab);
                        ret.Add(ab);
                    }
                    else
                    {
                        toBeAdded.Add(ab);
                    }
                }

                if (toBeAdded.Any())
                {
                    _context.AccountBalances.AddRange(toBeAdded);
                    _context.SaveChanges();
                }

                if (toBeUpdated.Any())
                {
                    var executionStrategy = _context.Database.CreateExecutionStrategy();
                    executionStrategy.Execute(
                        () =>
                        {
                            using (var transaction = UnitOfWork.BeginTransaction())
                            {
                                foreach (var a in toBeUpdated)
                                {
                                    try
                                    {
                                        var dbAccount =
                                            _context.AccountBalances.First(x =>
                                                x.ClientCuit == a.ClientCuit && x.Product == a.Product);
                                        UpdateAccountBalanceFromDb(dbAccount, a);
                                        _context.Entry(dbAccount).State = EntityState.Modified;
                                        _context.AccountBalances.Update(dbAccount);
                                        UnitOfWork.SaveChanges();
                                    }
                                    catch (Exception ex)
                                    {
                                        Log.Error(ex, "Hubo un error al actualizar el balance de cuenta: {@balance}", a);
                                    }
                                }
                                UnitOfWork.Commit(transaction);
                            }
                        });
                }
            }
            catch (Exception ex)
            {
                Log.Error(ex, "Hubo un error al guardar los balances de cuenta");
            }
            return ret;
        }

        private void UpdateAccountBalanceFromDb(AccountBalance dbAccount, AccountBalance updatedAccount)
        {
            dbAccount.TotalDebtAmount = updatedAccount.TotalDebtAmount;
            dbAccount.FuturePaymentsCount = updatedAccount.FuturePaymentsCount;
            dbAccount.FuturePaymentsAmountUSD = updatedAccount.FuturePaymentsAmountUSD;
            dbAccount.OverduePaymentDate = updatedAccount.OverduePaymentDate;
            dbAccount.OverduePaymentsCount = updatedAccount.OverduePaymentsCount;
            dbAccount.OverduePaymentsAmountUSD = updatedAccount.OverduePaymentsAmountUSD;
            dbAccount.PaidPaymentsCount = updatedAccount.PaidPaymentsCount;
            dbAccount.PaidPaymentsAmountUSD = updatedAccount.PaidPaymentsAmountUSD;
            dbAccount.BusinessUnit = updatedAccount.BusinessUnit;
            dbAccount.ClientReference = updatedAccount.ClientReference;
            dbAccount.RazonSocial = updatedAccount.RazonSocial;

            //Si esta en mora y pasa a al dia y esta como contactado hay que ponerlo como no contactado. Si esta como no contactado queda asi.
            if (dbAccount.Balance == AccountBalance.EBalance.Mora && updatedAccount.Balance == AccountBalance.EBalance.AlDia)
            {
                if (dbAccount.ContactStatus == AccountBalance.EContactStatus.Contactado)
                    dbAccount.ContactStatus = AccountBalance.EContactStatus.NoContactado;
                dbAccount.Department = AccountBalance.EDepartment.CuentasACobrar;

                AddLog(dbAccount.Id, AcountBalanceEntity.Department, new User { Id = "SYSTEM", Email = "SYSTEM" });
            }

            //Si esta al dia y vuelve a mora se pone como no contactado hasta que tenga un contacto con fecha mayor o igual a la fecha de mora.
            if (dbAccount.Balance == AccountBalance.EBalance.AlDia && updatedAccount.Balance == AccountBalance.EBalance.Mora)
                dbAccount.ContactStatus = AccountBalance.EContactStatus.NoContactado;
            dbAccount.Balance = updatedAccount.Balance;

            dbAccount.DelayStatus = updatedAccount.Balance == AccountBalance.EBalance.AlDia ? null : dbAccount.DelayStatus;
            if (updatedAccount.PublishDebt ==
                null) //These accountBalances come from already paid debts and have no balances, therefore have no publishDebt info
                updatedAccount.PublishDebt = "Y"; //By design this will be their default value

            //if the previous department wasnt external and now it is, publishdebt must be false ("N")
            if (dbAccount.Department != updatedAccount.Department &&
                updatedAccount.Department == AccountBalance.EDepartment.Externo)
                updatedAccount.PublishDebt = "N";

            if (dbAccount.PublishDebt != updatedAccount.PublishDebt)
            {
                dbAccount.PublishDebt = updatedAccount.PublishDebt;
                try
                {
                    var paymentService = _serviceProvider.GetService<IPaymentService>();
                    dbAccount.PublishDebt =
                        paymentService.UpdatePublishDebt(dbAccount.ClientCuit, dbAccount.Product, dbAccount.PublishDebt);
                    AddLog(dbAccount.Id, AcountBalanceEntity.PublishDebt, new User { Id = "SYSTEM", Email = "SYSTEM" });
                }
                catch (Exception ex)
                {
                    Log.Error(ex, "Hubo un error al actualizar publicacion de deuda en SGF. accountBalanceId: {id}, nroCuit: {cuit}, product: {prod}, {updatedPublishDebt}", dbAccount.Id, dbAccount.ClientCuit, dbAccount.Product, updatedAccount.PublishDebt);
                }
            }

            if (dbAccount.WorkStarted == null) //Default value should be N
                dbAccount.WorkStarted = "N";

            // Chequea si pasa a Legales
            CheckLegales(updatedAccount.Department, dbAccount.Id);
        }

        public void CleanLegalesNotification()
        {
            _context.Database.ExecuteSqlRaw("TRUNCATE TABLE [DepartmentChangeNotifications]");
        }

        public IEnumerable<string> GetAllBU(bool isExternal)
        {
            var result = new List<string>();
            if (isExternal)
            {
                result = _context.AccountBalances.Where(x => x.Department == AccountBalance.EDepartment.Externo && x.PublishDebt.Equals("N")).Select(x => x.BusinessUnit).Distinct().ToList();
            }
            else
            {
                result = _context.AccountBalances.Select(x => x.BusinessUnit).Distinct().ToList();
            }
            return result;
        }

        public IEnumerable<string> GetPropertyCodesByCuits(IEnumerable<string> cuits)
        {
            var result = new List<string>();

            var accountBalances = _context.AccountBalances.Where(x => cuits.Contains(x.ClientCuit)).ToList();
            if (accountBalances.Any())
            {
                result.AddRange(accountBalances.Select(x => x.Product).Distinct());
            }

            return result;
        }

        public List<AccountBalance> GelAllAccountBalanceWithOutCVU()
        {
            var ret = new List<AccountBalance>();
            try
            {
                if (_itauCvuConfig.EnableCustomItauCvu)
                {
                    foreach (var enabledBU in _itauCvuConfig.EnabledBUs)
                    {
                        try
                        {
                            var AccountBalancesFromBU = _context.AccountBalances
                            .Include(it => it.CvuEntities)
                            .Where(it => it.CvuEntities.Count == 0 && it.BusinessUnit == enabledBU.BusinessUnit)
                            .ToList();

                            ret.AddRange(AccountBalancesFromBU
                                .Where(it => enabledBU.EnabledAccountBalances.Exists(x => x.ClientCuit == it.ClientCuit && x.Product == it.Product))
                                .ToList());
                        }
                        catch (Exception ex)
                        {
                            Log.Error($"Error al momento de buscar accountBalances para {enabledBU}", ex);
                            continue;
                        }
                    }
                }
                else
                {
                    ret = _context.AccountBalances.Include(it => it.CvuEntities)
                    .Where(it => it.CvuEntities.Count == 0).ToList();
                }
            }
            catch (Exception e)
            {
                Log.Error("Error en GelAllAccountBalanceWithOutCVU: {@e}", e);
            }

            return ret;
        }


        private void AddLog(int id, string modifiedField, User user)
        {
            _userChangesLogRepository.Add(new UserChangesLog
            {
                EntityId = id,
                ModifiedEntity = CobraEntity.AccountBalance,
                ModifiedField = nameof(modifiedField),
                UserId = user.Id,
                UserEmail = user.Email,
                ModifyDate = LocalDateTime.GetDateTimeNow()

            });
        }

        public List<AccountBalance> GetAllByBusinessUnit(string buName)
        {
            return _context.AccountBalances.Where(x => x.BusinessUnit == buName).ToList();
        }

        public List<AccountBalance> GetAll(Expression<Func<AccountBalance, bool>> predicate)
        {
            return _context.AccountBalances.Where(predicate).ToList();
        }

        public async Task UpdateAll(List<AccountBalance> accountBalances)
        {
            _context.AccountBalances.UpdateRange(accountBalances);
            await _context.SaveChangesAsync();
        }
    }
}
