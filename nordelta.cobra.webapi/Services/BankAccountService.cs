using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Security.Cryptography.X509Certificates;
using System.ServiceModel;
using System.Threading.Tasks;
using CuentaServiceItau;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Options;
using Newtonsoft.Json;
using nordelta.cobra.webapi.Configuration;
using nordelta.cobra.webapi.Models;
using nordelta.cobra.webapi.Models.ValueObject.Certificate;
using nordelta.cobra.webapi.Repositories.Contracts;
using nordelta.cobra.webapi.Services.Contracts;
using nordelta.cobra.webapi.Services.DTOs;
using nordelta.cobra.webapi.Utils;
using Serilog;

namespace nordelta.cobra.webapi.Services
{
    public class BankAccountService : IBankAccountService
    {
        private readonly IBankAccountRepository _bankAccountRepository;
        private readonly IOptionsMonitor<ItauWCFConfiguration> _itauConfiguration;
        private readonly IConfiguration _configuration;
        private readonly IUserRepository _userRepository;
        private readonly IUserChangesLogRepository _userChangesLogRepository;

        private readonly List<CertificateItem> _certificateItems;
        //private readonly IOptionsMonitor<ItauCertificateConfig> _itauCertConfiguration;

        public BankAccountService(IBankAccountRepository bankAccountRepository,
                                  IOptionsMonitor<ItauWCFConfiguration> itauConfiguration,
                                  IConfiguration configuration,
                                  IUserRepository userRepository,
                                  IUserChangesLogRepository userChangesLogRepository
        )
        {
            _bankAccountRepository = bankAccountRepository;
            _itauConfiguration = itauConfiguration;
            //_itauCertConfiguration = itauCertConfiguration;
            _configuration = configuration;
            _certificateItems = _configuration.GetSection("ServiceConfiguration:CertificateSettings:CertificateItems").Get<List<CertificateItem>>();
            _userRepository = userRepository;
            _userChangesLogRepository = userChangesLogRepository;
            foreach (var certificateItem in _certificateItems)
                certificateItem.Password = AesManager.GetPassword(certificateItem.Password, _configuration.GetSection("SecretKeyCertificate").Value);
        }

        public List<BankAccount> GetBankAccountsForClient(string clientCuit, string accountNumber = "")
        {
            return _bankAccountRepository.All(clientCuit, accountNumber);
        }

        public List<BankAccount> GetBankAccountsForClient(List<string> userAdditionalCuits)
        {
            return _bankAccountRepository.All(userAdditionalCuits);
        }

        public ValidateBankAccountResponse ValidateBankAccount(string cbu, string cuit)
        {
            var itauMockEnabled = _configuration.GetSection("ServiceConfiguration").GetSection("EnableItauMock")
                .Get<bool>();
            if (itauMockEnabled)
            {
                var result = new ValidateBankAccountResponse()
                {
                    DenominacionCuit = "Mock Result",
                    Validacion = "CBU_OK",
                    NroCuenta = cbu,
                    Cuit = cuit,
                    Currency = Currency.ARS
                };
                return result;
            }
            if (!CuitValidator.IsValid(cuit)) throw new Exception("Invalid cuit");
            if (!CbuValidator.IsValid(cbu)) throw new Exception("Invalid cbu");

            // Validacion y obtencion datos cuenta contra Itau...
            var cuentaServiceConfiguration = _itauConfiguration.Get(ItauWCFConfiguration.CuentaServiceConfiguration);
            var itauCertificateConfig = GetItauCertificateConfig();

            var channelFactory = GetConfiguratedChannel<Cuenta>(cuentaServiceConfiguration, itauCertificateConfig);

            Cuenta serviceClient = channelFactory.CreateChannel();

            var validarCBURequest = new ValidarCBURequest()
            {
                body = new bodyRequestType14()
                {
                    ValidarCBU = new ValidarCBUType()
                    {
                        idCanalBanco = "259", //nose que es esto.
                        numeroCBU = cbu,
                        numeroCUIT = cuit
                    }
                },
                header = new headerType()
                {
                    clave_mensaje = new clave_mensajeType()
                    {
                        id_requerimiento = "7012",
                        host_origen = "0.0.0.0",
                        id_canal = "W",
                    },
                    info_requerimiento = new info_requerimientoType()
                    {
                        id_organizacion = "Itau",
                        codigo_sucursal = "0000",
                        fechahora_mensaje = LocalDateTime.GetDateTimeNow().ToString("yyyy-MM-dd-HH:mm:ss.fffff"),
                    },
                    seguridad = new seguridadType()
                    {
                        token = new tokenType()
                        {
                            passwordtoken = new passwordtokenType()
                            {
                                clave = "x",
                                id_usuario = "x"
                            }
                        }
                    }
                }
            };

            var response = serviceClient.ValidarCBUAsync(validarCBURequest).Result;

            if (response.header.transaccion.estado.Equals("Exito"))
            {
                var res = ((ValidarCBUResponseType)response.body.Item).cuenta;

                var result = new ValidateBankAccountResponse()
                {
                    DenominacionCuit = res.denominacionCuit,
                    Validacion = res.validacion,
                    NroCuenta = res.nroCuenta,
                    Cuit = cuit,
                    Currency = GetCurrency(res.tipoMoneda)
                };
                Log.Information("AddBankAccount: request:{@resquest} response: {@response}", validarCBURequest, response);
                return result;
            }
            else
            {
                Log.Information("AddBankAccount NotEqualsExito, request:{@resquest} response: {@response}", validarCBURequest, response);
                return new ValidateBankAccountResponse()
                {
                    Validacion = ((CuentaServiceItau.ExcepcionType)response.body.Item).detalle.descripcion
                };
            }
        }

        public BankAccount GetBankAccountFromCbu(string clientCuit, string cbu, Currency currency, string accountNumber="")
        {
            return _bankAccountRepository.GetAccountForClient(clientCuit, cbu, currency);
        }

        public BankAccount AddBankAccount(string clientCuit, string cbu, string cuit, Currency currency, User user)
        {
            try
            {
                var newBankAccount = new BankAccount()
                {
                    Cbu = cbu,
                    Cuit = cuit,
                    Currency = currency,
                    Status = BankAccountStatus.Approved,
                    ClientCuit = clientCuit,
                };
                if (user.IsForeignCuit)
                {
                    newBankAccount.ClientAccountNumber = user.AccountNumber;
                }

                _bankAccountRepository.Add(newBankAccount,
                    new User
                    {
                        Email = string.IsNullOrEmpty(user.SupportUserEmail) ? user.Email : user.SupportUserEmail,
                        Id = string.IsNullOrEmpty(user.SupportUserId) ? user.Id.ToString() : user.SupportUserId
                    });


                return newBankAccount;
            }
            catch (Exception ex)
            {
                Log.Error(ex, "Error in AddBankAccount.");
                throw;
            }

        }

        public bool DeleteBankAccount(int id, User user)
        {

            BankAccount bankAccount = _bankAccountRepository.Get(id);
            if (bankAccount == null)
            {
                Log.Error($"Error: No se encontro bank account.- Id: {id}.");
                return false;
            }
            if (user == null)
            {
                Log.Error($"Error: No se encontro usuario.- Cuit: {bankAccount.ClientCuit}.");
                return false;
            }


            return _bankAccountRepository.Delete(bankAccount.Id,
                new User
                {
                    Email = string.IsNullOrEmpty(user.SupportUserEmail) ? user.Email : user.SupportUserEmail,
                    Id = string.IsNullOrEmpty(user.SupportUserId) ? user.Id.ToString() : user.SupportUserId
                });



        }

        private Currency GetCurrency(string currency)
        {
            if (currency == "$")
                return Currency.ARS;
            if (currency == "USD")
                return Currency.USD;

            //...
            return Currency.ARS;
        }

        private CertificateItem GetItauCertificateConfig(string vendorCuit = "")
        {
            return _certificateItems.SingleOrDefault(it => it.VendorCuit == vendorCuit);
        }

        private static ChannelFactory<T> GetConfiguratedChannel<T>(ItauWCFConfiguration wcfServicesConfig, CertificateItem certificateConfig)
        {

            BasicHttpsBinding binding = new BasicHttpsBinding
            {
                Security =
                {
                    Mode = BasicHttpsSecurityMode.Transport,
                    Transport = {ClientCredentialType = HttpClientCredentialType.Certificate}
                }
            };

            string certificationPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Certificates",
                certificateConfig.Name);
            X509Certificate2 certificate = new X509Certificate2(certificationPath, certificateConfig.Password,
                X509KeyStorageFlags.Exportable | X509KeyStorageFlags.MachineKeySet | X509KeyStorageFlags.PersistKeySet);

            EndpointAddress endpoint = new EndpointAddress(new Uri(wcfServicesConfig.EndpointUrl));

            ChannelFactory<T> channelFactory = new ChannelFactory<T>(binding, endpoint);
            channelFactory.Credentials.ClientCertificate.Certificate = certificate;

            return channelFactory;
        }
    }
}
