using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Extensions.Options;
using nordelta.cobra.webapi.Configuration;
using nordelta.cobra.webapi.Services.Contracts;
using nordelta.cobra.webapi.Services.DTOs;
using RestSharp;
using Serilog;

namespace nordelta.cobra.webapi.Services
{
    public class ClientProfileService : IClientProfileService
    {
        private readonly IRestClient _restClient;
        private readonly IOptionsMonitor<ApiServicesConfig> _apiServicesConfig;
        public ClientProfileService(IRestClient restClient, IOptionsMonitor<ApiServicesConfig> options)
        {
            _restClient = restClient;
            _apiServicesConfig = options;
        }

        public List<ClientProfileControlDto> GetClientProfileControl()
        {
            _restClient.BaseUrl = new Uri(_apiServicesConfig.Get(ApiServicesConfig.SgfApi).Url);
            RestRequest request = new RestRequest("/Cliente/ObtenerControlPerfilCliente", Method.GET);
            request.AddHeader("Token", _apiServicesConfig.Get(ApiServicesConfig.SgfApi).Token);

            try
            {
                IRestResponse<List<ClientProfileControlDto>> clientProfileControlResponse = _restClient.Execute<List<ClientProfileControlDto>>(request);
                if (!clientProfileControlResponse.IsSuccessful)
                {
                    Log.Error("No se pudo obtener Control de Perfil de Clientes.\n Request: {@request} \n Response: {@response}", request, clientProfileControlResponse);
                }

                List<ClientProfileControlDto> result = new List<ClientProfileControlDto>();

                if (clientProfileControlResponse.Data != null)
                {
                    result = clientProfileControlResponse.Data.ToList();
                }

                return result;
            }
            catch (Exception e)
            {
                Log.Error(@"GetClientProfileControl, 
                    Type:GetClientProfileControl,
                    Description: Error fetching ClientProfileControl:
                    request: {@request},
                    response: {@error}", request, e);
                throw new Exception("Error al obtener el Control de Perfil de Clientes Sgf: " + e.Message);
            }

        }
    }
}
