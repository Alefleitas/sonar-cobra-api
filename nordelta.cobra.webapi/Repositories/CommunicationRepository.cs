using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.EntityFrameworkCore;
using nordelta.cobra.webapi.Models;
using nordelta.cobra.webapi.Repositories.Contexts;
using nordelta.cobra.webapi.Repositories.Contracts;
using nordelta.cobra.webapi.Utils;
using Serilog;

namespace nordelta.cobra.webapi.Repositories
{
    public class CommunicationRepository : ICommunicationRepository
    {
        private readonly RelationalDbContext _context;
        private readonly IUserChangesLogRepository _userChangesLogRepository;
        public IUnitOfWork UnitOfWork => _context;

        public CommunicationRepository(RelationalDbContext context, IUserChangesLogRepository userChangesLogRepository)
        {
            this._context = context;
            _userChangesLogRepository = userChangesLogRepository;

        }

        public List<Communication> GetAll()
        {
            return this._context.Communications.Include(x => x.ContactDetail).Include(y => y.AccountBalance).ToList();
        }
        public List<Communication> GetAllForAccountBalance(int accountBalanceId)
        {
            return this._context.Communications.Include(x => x.ContactDetail).Where(x => x.AccountBalanceId == accountBalanceId).ToList();
        }

        public Communication InsertOrUpdate(Communication comm)
        {
            try
            {
                UnitOfWork.RunWithExecutionStrategy(() =>
                {
                    if (comm.Id != 0)
                        this._context.Communications.Update(comm);
                    else
                        this._context.Communications.Add(comm);

                    this.UnitOfWork.SaveChanges();

                    AddLog(new UserChangesLog
                        {

                            EntityId = comm.Id,
                            ModifiedEntity = CobraEntity.Communication,
                            ModifiedField = nameof(comm.Id),
                            UserEmail = string.IsNullOrEmpty(comm.SsoUser.Email) ? "SYSTEM" : comm.SsoUser.Email,
                            UserId = string.IsNullOrEmpty(comm.SsoUser.IdApplicationUser) ? "SYSTEM" : comm.SsoUser.IdApplicationUser,
                            ModifyDate = LocalDateTime.GetDateTimeNow()
                        }
                    );
                });
            }
            catch (Exception ex)
            {
                Log.Logger.Error(ex, "[ERROR]: Error al insertar/actualizar la Communication con Id {id}", comm.Id);
            }

            return comm;
        }

        public bool Insert(IEnumerable<Communication> communications)
        {
            this._context.AddRange(communications);
            var saved = this._context.SaveChanges();

            return saved > 0;
        }

        public bool Delete(int id, User user)
        {
            var comm = this._context.Communications.Include(x => x.AccountBalance).SingleOrDefault(y => y.Id == id);
            var committed = false;
            if (comm == null) return committed;
            var executionStrategy = _context.Database.CreateExecutionStrategy();
            executionStrategy.Execute(() =>
            {
                using (var transaction = UnitOfWork.BeginTransaction())
                {
                    try
                    {
                        AddLog(new UserChangesLog
                        {
                            EntityId = id,
                            ModifiedEntity = CobraEntity.Communication,
                            ModifiedField = nameof(id),
                            UserEmail = string.IsNullOrEmpty(user.Email) ? "SYSTEM" : user.Email,
                            UserId = string.IsNullOrEmpty(user.Id) ? "SYSTEM" : user.Id,
                            ModifyDate = LocalDateTime.GetDateTimeNow()
                        });
                        this._context.Remove(comm);
                        this.UnitOfWork.SaveChanges();

                        this.UnitOfWork.Commit(transaction);
                        committed = true;

                    }
                    catch (Exception ex)
                    {
                        Log.Logger.Error(ex, "[ERROR]: Error al eliminar la Communication con Id {id}", id);
                    }
                }
            });

            return committed;
        }

        public Communication GetById(int id)
        {
            return this._context.Communications.Include(x => x.AccountBalance).SingleOrDefault(y => y.Id == id);
        }

        public bool ToggleTemplate(int id)
        {
            var template = this._context.Template.SingleOrDefault(x => x.Id == id);
            template.Disabled = !template.Disabled;
            this._context.SaveChanges();
            return true;
        }


        private void AddLog(UserChangesLog user)
        {
            _userChangesLogRepository.Add(user);
        }
    }
}
