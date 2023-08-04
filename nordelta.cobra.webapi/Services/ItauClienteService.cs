﻿using Itau;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Options;
using Newtonsoft.Json;
using nordelta.cobra.webapi.Configuration;
using nordelta.cobra.webapi.Connected_Services.Itau.ArchivosCmlServiceItau.Constants;
using nordelta.cobra.webapi.Models;
using nordelta.cobra.webapi.Models.ValueObject.Certificate;
using nordelta.cobra.webapi.Models.ValueObject.ItauPsp;
using nordelta.cobra.webapi.Repositories.Contracts;
using nordelta.cobra.webapi.Services.Contracts;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Security.Cryptography.X509Certificates;
using System.ServiceModel;
using System.Threading.Tasks;

namespace nordelta.cobra.webapi.Services;

public class ItauClienteService : IItauClienteService
{
    private readonly IAccountBalanceRepository _accountBalanceRepository;
    private readonly IPublishClientRepository _publishClientRepository;
    private readonly IUserRepository _userRepository;
    private readonly IConfiguration _configuration;
    private readonly ItauWCFConfiguration _itauWcfCliente;
    private readonly List<CertificateItem> _certificateItems;
    private readonly List<ItauPspItem> _itauPspItems;

    public ItauClienteService(
        IAccountBalanceRepository accountBalanceRepository,
        IPublishClientRepository publishClientRepository,
        IUserRepository userRepository,
        IOptionsMonitor<List<CertificateItem>> certificateItems,
        IOptionsMonitor<List<ItauPspItem>> itauPspItems,
        IOptionsMonitor<ItauWCFConfiguration> itauWCFConfig,
        IConfiguration configuration
        )
    {
        _accountBalanceRepository = accountBalanceRepository;
        _publishClientRepository = publishClientRepository;
        _userRepository = userRepository;
        _configuration = configuration;
        _itauWcfCliente = itauWCFConfig.Get(ItauWCFConfiguration.ClienteServiceConfiguration);
        _certificateItems = certificateItems.Get(CertificateItem.CertificateItems);
        _itauPspItems = itauPspItems.Get(ItauPspItem.ItauPspItems);
    }

    public async Task ClientMassPublish()
    {
        if (!_configuration.GetSection("EnableClientEcheqMassCreation").Get<bool>())
            return;

        var businessUnits = _accountBalanceRepository.GetAllBU(false);

        foreach (var businessUnit in businessUnits)
        {
            try
            {
                var itauPspItem = _itauPspItems.FirstOrDefault(x => x.ProductoNumero == ItauPspItem.ServicioDeCobranzas && x.Name == businessUnit);

                if (itauPspItem is null)
                {
                    Serilog.Log.Information("ClientMassPublish(): La businessUnit {businessUnit} no esta habilitado para publicar clientes a Itau", businessUnit);
                    continue;
                }

                var accountBalancesFromBu = _accountBalanceRepository.GetAllByBusinessUnit(businessUnit);
                var clientIds = accountBalancesFromBu.Select(x => x.ClientId).Distinct().ToList();

                await PublishClient(itauPspItem, clientIds);
            }
            catch (Exception ex)
            {
                Serilog.Log.Error(ex, "ClientMassPublish(): Ocurrió un error al intentar realizar la publicación masiva de clientes a Itau para la BU {bu}", businessUnit);
            }
        }
    }

    public async Task PublishClient(ItauPspItem itauPspItem, List<string> clientIds)
    {
        Serilog.Log.Information("Comienza la publicación de clientes para ItauPspItem: {@itauPspItem}", itauPspItem);

        var enabledCuits = _configuration.GetSection("PublishClientItauCustom:TestCuits").Get<List<string>>();
        var certConfig = GetItauCertificateConfig(itauPspItem.VendorCuit);

        var channelFactoryCliente = GetConfiguratedChannel<CLIENTE>(_itauWcfCliente, certConfig);
        var serviceClient = channelFactoryCliente.CreateChannel();

        var clientes = _userRepository.GetUsersByIds(clientIds);

        if (enabledCuits is not null)
        {
            clientes = clientes.Where(x => enabledCuits.Contains(x.Cuit)).ToList();
        }

        var convenio = new convenio
        {
            cuit = itauPspItem.VendorCuit,
            producto = new producto
            {
                numero = itauPspItem.ProductoNumero
            },
            numero = itauPspItem.ConvenioNumero
        };

        foreach (var cliente in clientes)
        {
            try
            {
                var publishClient = await _publishClientRepository.GetOneAsync(x => x.ClientId == cliente.IdApplicationUser && x.CuitBU == itauPspItem.VendorCuit);

                if (publishClient is not null && publishClient.Status == EStatusPublishClient.PUBLICADO)
                {
                    continue;
                }
                else
                {
                    publishClient ??= new PublishClient
                    {
                        ClientId = cliente.IdApplicationUser,
                        CuitBU = itauPspItem.VendorCuit,
                        Source = PaymentSource.Itau
                    };
                }

                var requestCliente = new cliente
                {
                    id = "CT" + cliente.Cuit.Trim(),   // ID = TIPO_DOC + NRO_DOC
                    documento = new documento
                    {
                        tipo = "CT",
                        numero = cliente.Cuit.Trim()
                    },
                    razonSocial = cliente.RazonSocial.Trim(),
                    mail = cliente.Email.Trim()
                };

                var request = new clientesComprobantesPublicacionRequest(new clientesComprobantesPublicacion
                {
                    convenio = convenio,
                    cliente = requestCliente
                });

                var response = await serviceClient.clientesComprobantesPublicacionAsync(request);

                // PC0474 : Cliente existente en Itaú 
                if (response.clientesComprobantesPublicacionResponse.retorno.codigo == CodigoRetorno.OkResult ||
                    response.clientesComprobantesPublicacionResponse.retorno.mensaje.Contains("PC0474"))
                {
                    publishClient.Status = EStatusPublishClient.PUBLICADO;
                    publishClient.Detail = response.clientesComprobantesPublicacionResponse.retorno.descripcion;
                    _ = publishClient.Id == 0 ? await _publishClientRepository.AddAsync(publishClient) :
                        await _publishClientRepository.UpdateAsync(publishClient);

                    Serilog.Log.Information("PublishClient(): Se publico cliente a Itaú " +
                        "\n request: {request}" +
                        "\n response: {response}", JsonConvert.SerializeObject(request), JsonConvert.SerializeObject(response));
                }
                else
                {
                    publishClient.Status = EStatusPublishClient.NO_PUBLICADO;
                    publishClient.Detail = response.clientesComprobantesPublicacionResponse.retorno.descripcion;
                    _ = publishClient.Id == 0 ? await _publishClientRepository.AddAsync(publishClient) :
                        await _publishClientRepository.UpdateAsync(publishClient);

                    Serilog.Log.Warning("PublishClient(): No se publico cliente a Itaú " +
                        "\n request: {request}" +
                        "\n response: {response}", JsonConvert.SerializeObject(request), JsonConvert.SerializeObject(response));
                }
            }
            catch (Exception ex)
            {
                Serilog.Log.Error(ex, "PublishClient(): Ocurrió un error al intenar publicar cliente a Itaú" +
                    "\n cliente: {@cliente}", cliente);
            }
        }
    }

    private CertificateItem GetItauCertificateConfig(string vendorCuit)
    {
        return _certificateItems.SingleOrDefault(it => it.VendorCuit == vendorCuit);
    }

    private static ChannelFactory<T> GetConfiguratedChannel<T>(ItauWCFConfiguration wcfServicesConfig,
        CertificateItem certificateConfig)
    {
        BasicHttpsBinding binding = new BasicHttpsBinding
        {
            Security =
                {
                    Mode = BasicHttpsSecurityMode.Transport,
                    Transport = {ClientCredentialType = HttpClientCredentialType.Certificate}
                }
        };

        var certificationPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory ?? string.Empty, "Certificates",
            certificateConfig.Name);
        var certificate = new X509Certificate2(certificationPath, certificateConfig.Password,
            X509KeyStorageFlags.Exportable | X509KeyStorageFlags.MachineKeySet | X509KeyStorageFlags.PersistKeySet);

        var endpoint = new EndpointAddress(new Uri(wcfServicesConfig.EndpointUrl));

        var channelFactory = new ChannelFactory<T>(binding, endpoint);
        channelFactory.Credentials.ClientCertificate.Certificate = certificate;

        return channelFactory;
    }
}