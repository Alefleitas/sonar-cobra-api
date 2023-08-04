using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Extensions.Options;
using nordelta.cobra.webapi.Configuration;
using nordelta.cobra.webapi.Models;
using nordelta.cobra.webapi.Repositories.Contracts;
using nordelta.cobra.webapi.Services.Contracts;
using nordelta.cobra.webapi.Services.DTOs;
using RestSharp;
using Serilog;

namespace nordelta.cobra.webapi.Services
{
    public class ContactDetailService: IContactDetailService
    {
        private readonly IRestClient _restClient;
        private readonly IOptionsMonitor<ApiServicesConfig> _apiServicesConfig;
        private readonly IContactDetailRepository _contactDetailRepository;
        public ContactDetailService(IContactDetailRepository contactDetailRepository, IRestClient restClient, IOptionsMonitor<ApiServicesConfig> options)
        {
            _restClient = restClient;
            _apiServicesConfig = options;
            _contactDetailRepository = contactDetailRepository;
        }
        public ContactDetail InsertOrUpdate(ContactDetail contactDetail, User user)
        {
            return _contactDetailRepository.InsertOrUpdate(contactDetail, user);
        }
        public List<ContactDetail> GetAllByUserId(string userId)
        {
            return _contactDetailRepository.GetAllContactDetailsByUserId(userId);
        }
        public bool Delete(int id, User user)
        {
            return _contactDetailRepository.Delete(id, user);
        }

        public List<ContactDetailDto> GetClienteDatosContactos(List<string> cuits, string codigoProducto = "")
        {
            var requestModel = new ClienteDetalleContactoRequest();
            requestModel.NroDocumentos.AddRange(cuits);

            _restClient.BaseUrl = new Uri(_apiServicesConfig.Get(ApiServicesConfig.SgfApi).Url);
            RestRequest request = new RestRequest("/Cliente/ObtenerDatosContactoClientes", Method.POST);
            request.AddHeader("Token", _apiServicesConfig.Get(ApiServicesConfig.SgfApi).Token);
            request.AddJsonBody(requestModel);

            try
            {
                IRestResponse<List<ContactDetailDto>> detalleContactoResponse = _restClient.Execute<List<ContactDetailDto>>(request);
                if (!detalleContactoResponse.IsSuccessful)
                {
                    Log.Error("No se pudo obtener Datos de Contactos.\n Request: {@request} \n Response: {@response}", request, detalleContactoResponse);
                }

                List<ContactDetailDto> result = new List<ContactDetailDto>();

                if (detalleContactoResponse.Data != null)
                {
                    if (String.IsNullOrEmpty(codigoProducto))
                        result = detalleContactoResponse.Data.ToList();
                    else result = detalleContactoResponse.Data.Where(x => x.Producto == codigoProducto).ToList();
                }

                return result;
            }
            catch (Exception e)
            {
                Log.Error(@"GetClienteDatosContactos, 
                    Type:GetClienteDatosContactos,
                    Description: Error fetching ClienteDatosContactos:
                    request: {@request},
                    response: {@error}", request, e);
                throw new Exception("Error al obtener los detalles de contactos del cliente del Sgf: " + e.Message);
            }

        }
    }
}
