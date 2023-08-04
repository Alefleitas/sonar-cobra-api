using nordelta.cobra.webapi.Models;
using nordelta.cobra.webapi.Services.DTOs;
using System.Collections.Generic;

namespace nordelta.cobra.webapi.Services.Contracts;

public interface IValidacionClienteService
{
    void SyncValidacionCliente();
    ValidacionCliente GetByCuitClientAndProductCode(string cuit, string product);
    IEnumerable<ValidacionClientesDto> GetValidacionClientesFromOracle();
}