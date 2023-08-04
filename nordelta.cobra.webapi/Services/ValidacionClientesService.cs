using AutoMapper;
using Microsoft.Extensions.Options;
using nordelta.cobra.webapi.Configuration;
using nordelta.cobra.webapi.Models;
using nordelta.cobra.webapi.Repositories.Contracts;
using nordelta.cobra.webapi.Services.Contracts;
using nordelta.cobra.webapi.Services.DTOs;
using RestSharp;
using Serilog;
using System;
using System.Collections.Generic;

namespace nordelta.cobra.webapi.Services;

public class ValidacionClientesService : IValidacionClienteService
{
    private readonly IRestClient _restClient;
    private readonly IOptionsMonitor<ApiServicesConfig> _apiServicesConfig;
    private readonly IValidacionClienteRepository _validacionClienteRepository;
    private readonly IMapper _mapper;

    public ValidacionClientesService(
        IRestClient restClient, 
        IOptionsMonitor<ApiServicesConfig> options, 
        IValidacionClienteRepository validacionClienteRepository,
        IMapper mapper
        )
    {
        _restClient = restClient;
        _apiServicesConfig = options;
        _validacionClienteRepository = validacionClienteRepository;
        _mapper = mapper;
    }

    public void SyncValidacionCliente()
    {
        try
        { 
            var validacionClienteDtos = GetValidacionClientesFromOracle();         
            var validacionClientes = _mapper.Map<IEnumerable<ValidacionClientesDto>, IEnumerable<ValidacionCliente>>(validacionClienteDtos);
            _validacionClienteRepository.Sync(validacionClientes);
        }
        catch (Exception ex)
        {
            Log.Error(ex, "Ocurrió un error al intentar sincronizar ValidacionCliente, Msg: {msg}", ex.Message);
            throw;
        }
    }
    
    public ValidacionCliente GetByCuitClientAndProductCode(string cuit, string product)
    {
        return _validacionClienteRepository.GetSingle(x => x.JgzzFiscalCode.Trim() == cuit.Trim() && 
                                                           x.LocAttribute1.Trim() == product.Trim() && 
                                                           x.DefaultRegistrationFlag.Trim() == "Y"); // Y : Deudas publicadas a COBRA
    }
    
    public IEnumerable<ValidacionClientesDto> GetValidacionClientesFromOracle()
    {
        _restClient.BaseUrl = new Uri(_apiServicesConfig.Get(ApiServicesConfig.SgfApi).Url);
        var request = new RestRequest("/Cliente/ObtenerValidacionClientes", Method.GET);
        request.AddHeader("Token", _apiServicesConfig.Get(ApiServicesConfig.SgfApi).Token);

        try
        {
            var validacionClientesResponse = _restClient.Execute<List<ValidacionClientesDto>>(request);
            if (!validacionClientesResponse.IsSuccessful)
            {
                Log.Error("No se pudo obtener Validacion Clientes.\n Request: {@request} \n Response: {@response}", request, validacionClientesResponse);
            }

            var result = new List<ValidacionClientesDto>();

            if (validacionClientesResponse.Data != null)
            {
                result = validacionClientesResponse.Data;
            }

            return result;
        }
        catch (Exception e)
        {
            Log.Error(@"GetValidacionClientesFromOracle, 
                    Type:ValidacionCliente,
                    Description: Error fetching ValidacionClientes:
                    request: {@request},
                    response: {@error}", request, e);
            throw new Exception("Error al obtener el ValidacionClientes de Clientes Sgf: " + e.Message);
        }
    }
}
