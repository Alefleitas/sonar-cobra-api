using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AutoMapper.QueryableExtensions;
using Microsoft.EntityFrameworkCore;
using nordelta.cobra.webapi.Models;
using nordelta.cobra.webapi.Repositories.Contexts;
using nordelta.cobra.webapi.Repositories.Contracts;
using nordelta.cobra.webapi.Utils;
using Serilog;

namespace nordelta.cobra.webapi.Repositories
{
    public class ContactDetailRepository : IContactDetailRepository
    {
        private readonly RelationalDbContext _context;
        private readonly IUserRepository _userRepository;
        private readonly IUserChangesLogRepository _userChangesLogRepository;

        public IUnitOfWork UnitOfWork => _context;

        public ContactDetailRepository(RelationalDbContext context, IUserRepository userRepository, IUserChangesLogRepository userChangesLogRepository)
        {
            this._context = context;
            _userRepository = userRepository;
            _userChangesLogRepository = userChangesLogRepository;
        }

        public ContactDetail InsertOrUpdate(ContactDetail contactDetail, User user)
        {
            var contactDetailExist = this._context.ContactDetails.Any(x => x.Id == contactDetail.Id);

            UnitOfWork.RunWithExecutionStrategy(() =>
            {
                if (contactDetailExist)
                {
                    _context.ContactDetails.Update(contactDetail);
                }
                else
                {
                    _context.ContactDetails.Add(contactDetail);
                }
                _context.SaveChanges();
                
                if (user == null)
                    Log.Warning($"No se encontro usuario. userId: {user.Id}.");
                else
                {
                    _userChangesLogRepository.Add(new UserChangesLog
                    {
                        EntityId = contactDetail.Id,
                        ModifiedEntity = CobraEntity.ContactDetail,
                        ModifiedField = nameof(contactDetail.Id),
                        ModifyDate = LocalDateTime.GetDateTimeNow(),
                        UserEmail = user.Email,
                        UserId = user.Id
                    });
                }
            });
            
            return contactDetail;
        }

        public ContactDetail GetById(int id)
        {
            return _context.ContactDetails.SingleOrDefault(x => x.Id == id);
        }
        public ContactDetail GetByCorreoElectronico(string correoElectronico)
        {
            return _context.ContactDetails.SingleOrDefault(x => x.Value == correoElectronico);
        }
        public List<ContactDetail> GetAllContactDetailsByUserId(string userId)
        {
            return _context.ContactDetails.AsNoTracking().Where(x => x.UserId.Equals(userId)).ToList();
        }

        public bool Delete(int id, User user)
        {
            var cDetail = _context.ContactDetails.SingleOrDefault(x => x.Id == id);
            try
            {
                if (cDetail == null) return false;

                if (user == null)
                    Log.Error($"Error: No se encontro usuario. Cuit: {user.Cuit}.");
                else
                {
                    UnitOfWork.RunWithExecutionStrategy(() =>
                    {
                        _userChangesLogRepository.Add(new UserChangesLog
                        {
                            EntityId = cDetail.Id,
                            ModifiedEntity = CobraEntity.ContactDetail,
                            ModifiedField = nameof(cDetail.Id),
                            ModifyDate = LocalDateTime.GetDateTimeNow(),
                            UserEmail = user.Email,
                            UserId = user.Id
                        });
                        _context.Remove(cDetail);
                        _context.SaveChanges();

                    });
                }
                return true;
            }
            catch (Exception ex)
            {
                Log.Logger.Error(ex, "Error al eliminar el Contact Detail con Id {id}", id);
                throw;
            }
        }
    }
}
