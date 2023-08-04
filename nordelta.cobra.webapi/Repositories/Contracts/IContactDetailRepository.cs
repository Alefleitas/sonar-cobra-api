
using System.Collections;
using System.Collections.Generic;
using nordelta.cobra.webapi.Models;
using nordelta.cobra.webapi.Services.DTOs;

namespace nordelta.cobra.webapi.Repositories.Contracts
{
    public interface IContactDetailRepository
    {
        IUnitOfWork UnitOfWork { get; }
        List<ContactDetail> GetAllContactDetailsByUserId(string userId);
        ContactDetail InsertOrUpdate(ContactDetail contactDetail, User user);
        bool Delete(int id, User user);
        ContactDetail GetById(int id);
        ContactDetail GetByCorreoElectronico(string correoElectronico);
    }
}
