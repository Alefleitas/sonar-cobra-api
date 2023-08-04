using System;
using nordelta.cobra.webapi.Models;
using nordelta.cobra.webapi.Repositories.Contexts;
using nordelta.cobra.webapi.Repositories.Contracts;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using nordelta.cobra.webapi.Utils;

namespace nordelta.cobra.webapi.Repositories
{
    public class RestrictionsListRepository : IRestrictionsListRepository
    {
        public IUnitOfWork UnitOfWork => _context;
        private readonly RelationalDbContext _context;
        private readonly IUserChangesLogRepository _userChangesLogRepository;

        public RestrictionsListRepository(RelationalDbContext context, IUserChangesLogRepository userChangesLogRepository)
        {
            _context = context;
            _userChangesLogRepository = userChangesLogRepository;
        }

        public LockAdvancePayments GetLockAdvancePayments()
        {
            return _context.LockAdvancePayment.First();
        }
        public bool SetLockAdvancePayments(bool action, User user)
        {
            LockAdvancePayments lockAdvancePayments = GetLockAdvancePayments();

            UnitOfWork.RunWithExecutionStrategy(() =>
            {
                lockAdvancePayments.LockedByUser = action;
                _context.SaveChanges();

                AddLog(new UserChangesLog
                    {
                        EntityId = action ? 1 : 0,
                        ModifiedEntity = CobraEntity.LockAdvancePayments,
                        ModifiedField = "Id",
                        UserEmail = user.Email,
                        UserId = user.Id,
                        ModifyDate = LocalDateTime.GetDateTimeNow()
                    }
                );

            });
           
            return lockAdvancePayments.LockedByUser;
        }
        public List<Restriction> GetCompleteRestrictionsList()
        {
            return _context.RestrictionsList.ToList();
        }
        public List<Restriction> GetRestrictionsListByUserId(string userId)
        {
            return _context.RestrictionsList.Where(x => x.UserId.Equals(userId)).ToList();
        }

        public void AddRestrictions(List<Restriction> restrictions)
        {
            _context.RestrictionsList.AddRange(restrictions);
            _context.SaveChanges();
        }

        public bool DeleteRestrictionsByUserId(string userId)
        {
            try
            {
                List<Restriction> restrictionsToDelete = _context.RestrictionsList
                    .Where(x => x.UserId.Equals(userId))
                    .ToList();
                if (restrictionsToDelete.Any())
                {
                    _context.RestrictionsList.RemoveRange(restrictionsToDelete);
                    _context.SaveChanges();
                    Serilog.Log.Information("Se han eliminado restricciones al Cliente con UserId {@id}", userId);
                }
                else
                {
                    Serilog.Log.Warning("No se encontró un Cliente con id {@id}", userId);
                }
                return true;
            }
            catch (Exception ex)
            {
                Serilog.Log.Error("Ocurrió un error eliminando restricciones con UserId {@id}: {@message}", userId, ex.Message);
                return false;
            }
        }

        private void AddLog(UserChangesLog userChanges)
        {
            _userChangesLogRepository.Add(userChanges);
        }
    }
}
