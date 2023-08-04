using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using nordelta.cobra.webapi.Controllers.ActionFilters;
using nordelta.cobra.webapi.Controllers.ViewModels;
using nordelta.cobra.webapi.Models;
using nordelta.cobra.webapi.Services.Contracts;

namespace nordelta.cobra.webapi.Controllers
{
    [AuthToken(EPermission.Access_CRM)]
    public class ContactDetailController : BaseApiController
    {
        private readonly IContactDetailService _contactDetailService;

        public ContactDetailController(IContactDetailService contactDetailService)
        {
            _contactDetailService = contactDetailService;
        }

        [HttpPost]
        public IActionResult CreateOrUpdate(ContactDetail contactDetail)
        {
            return ExecuteWithErrorHandling(() =>
            {
                User user = ((User)JsonConvert.DeserializeObject(HttpContext.Request.Headers["user"], typeof(User)));
                user.Id = string.IsNullOrEmpty(user.SupportUserId) ? user.Id : user.SupportUserId;

                var result = _contactDetailService.InsertOrUpdate(contactDetail, user);
                return Ok(result);
            });
        }

        [HttpPost]
        public IActionResult GetAllByUserId(contactDetailClientContactDetailViewModel model)
        {
            ClienteDetalleContactoViewModel clienContactDetail = new ClienteDetalleContactoViewModel {
                codigoProducto = model.codigoProducto,
                cuits = model.cuits
            };

            return ExecuteWithErrorHandling(() =>
            {
                var clientContactDetails = _contactDetailService.GetClienteDatosContactos(model.cuits, model.codigoProducto);
                var contactDetails = _contactDetailService.GetAllByUserId(model.userId);
                List<ContactDetail> result = new List<ContactDetail>();

                // Descarto los contactos de oracle que se guardarón en la DB
                foreach (var contactDetail in contactDetails) 
                {
                    if (!clientContactDetails.Exists(x => x.EmailAddress == contactDetail.Value))
                    {
                        result.Add(contactDetail);
                    }
                }

                return Ok(result);
        });
        }

        [HttpDelete("{id}")]
        public IActionResult Delete(int id)
        {
            return ExecuteWithErrorHandling(() =>
            {
                User user = ((User)JsonConvert.DeserializeObject(HttpContext.Request.Headers["user"], typeof(User)));
                user.Id = string.IsNullOrEmpty(user.SupportUserId) ? user.Id : user.SupportUserId;
                var result = _contactDetailService.Delete(id, user);
                return Ok(result);
            });
        }

        [HttpPost]
        public IActionResult GetClienteDetalleContacto(ClienteDetalleContactoViewModel model)
        {
            return ExecuteWithErrorHandling(() =>
            {
                var result = _contactDetailService.GetClienteDatosContactos(model.cuits, model.codigoProducto);
                return Ok(result);
            });
        }
    }
}