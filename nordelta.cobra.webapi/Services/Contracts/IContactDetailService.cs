using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using nordelta.cobra.webapi.Models;
using nordelta.cobra.webapi.Services.DTOs;

namespace nordelta.cobra.webapi.Services.Contracts
{
    public interface IContactDetailService
    {
        ContactDetail InsertOrUpdate(ContactDetail contactDetail, User user);
        List<ContactDetail> GetAllByUserId(string userId);
        bool Delete(int id, User user);
        List<ContactDetailDto> GetClienteDatosContactos(List<string> cuits, string codigoProducto = "");
    }
}
