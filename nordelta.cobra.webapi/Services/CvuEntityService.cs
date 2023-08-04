using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;
using nordelta.cobra.webapi.Models;
using nordelta.cobra.webapi.Repositories.Contracts;
using nordelta.cobra.webapi.Services.Contracts;
using nordelta.cobra.webapi.Services.DTOs;
using nordelta.cobra.webapi.Utils;

namespace nordelta.cobra.webapi.Services
{
    public class CvuEntityService : ICvuEntityService
    {
        private readonly ICvuEntityRepository _cvuEntityRepository;
        private readonly ICompanyRepository _companyRepository;
        private readonly IItauService _itauService;
        private readonly IUserRepository _userRepository;
        private readonly IAccountBalanceRepository _accountBalanceRepository;
        private readonly IPaymentService _paymentService;

        public CvuEntityService(
            ICvuEntityRepository cvuEntityRepository,
            ICompanyRepository companyRepository,
            IItauService itauService,
            IUserRepository userRepository,
            IAccountBalanceRepository accountBalanceRepository,
            IPaymentService paymentService)
        {
            _cvuEntityRepository = cvuEntityRepository;
            _companyRepository = companyRepository;
            _itauService = itauService;
            _userRepository = userRepository;
            _accountBalanceRepository = accountBalanceRepository;
            _paymentService = paymentService;
        }

        private CvuEntityDto CreateCvuDto(TransactionResultDto transactionResult, int accountBalanceId)
        {
            if (transactionResult == null || transactionResult.TransactionType != TransactionType.createCvuTransaction)
                throw new Exception(
                    $"Ocurrio un error al intentar la operación con Itau Api. \n Respuesta: {JsonConvert.SerializeObject(transactionResult)}");
            var newCvu = new CvuEntityDto()
            {
                ItauCreationTransactionId = transactionResult.TransactionId,
                AccountBalanceId = accountBalanceId
            };

            return newCvu;
        }

        public bool CompleteCvuCreationForTransactionId(string transactionId, TransactionResultDto transactionResult)
        {
            try
            {
                Serilog.Log.Information("Comienza el proceso de consiliación de Registro");

                var cvu = _cvuEntityRepository.GetSingle(
                    it => it.ItauCreationTransactionId == transactionId &&
                          it.Status == CvuEntityStatus.RegistrationStarted);

                if (cvu == null)
                {
                    throw new Exception($@"Error al intentar encontrar CVU para la TransactionID: ${transactionId} en estado Started");
                }
                if (!string.IsNullOrEmpty(transactionResult.Cvu))
                {
                    cvu.Status = CvuEntityStatus.RegistrationComplete;
                    cvu.Alias = transactionResult.Alias;
                    cvu.CvuValue = transactionResult.Cvu;
                    var ret = _cvuEntityRepository.Update(cvu);
                    Serilog.Log.Information(
                        $"Proceso creacion CVU finalizado mensaje recibido de Itau: {JsonConvert.SerializeObject(transactionResult)}");
                    return ret;
                }
                else
                {
                    Serilog.Log.Information($"Proceso creacion CVU falló por datos nulos: {JsonConvert.SerializeObject(transactionResult)}");
                    return false;
                }
            }
            catch (Exception e)
            {
                Serilog.Log.Error("Error en CompleteCvuCreation Ex: @{e} \n TransactionResult: @{tr}", e, transactionResult);
            }

            return false;
        }

        public bool CreateCvu(CvuEntityDto newCvu)
        {
            var cvuToAdd = new CvuEntity
            {
                Status = CvuEntityStatus.RegistrationStarted,
                AccountBalanceId = newCvu.AccountBalanceId,
                CreationDate = LocalDateTime.GetDateTimeNow(),
                ItauCreationTransactionId = newCvu.ItauCreationTransactionId,
                Currency = newCvu.Currency
            };

            return _cvuEntityRepository.Insert(cvuToAdd);
        }

        public bool BeginCvuTransactionCreation(int accountBalanceId, string productCode, string cuitBu, string clienCuit)
        {
            try
            {
                var user = _userRepository.GetSsoUserByCuit(clienCuit);
                if (user == null)
                {
                    throw new Exception($"Error: Al obtener usuario de SSO para el Cuit : {clienCuit}");
                }

                var registerCvuDto = new RegisterCvuDto
                {
                    ClientId = accountBalanceId + clienCuit,
                    Currency = Currency.ARS,
                    Cuit = clienCuit,
                    HolderName = user.RazonSocial,
                    PersonType = PersonTypeCalc.GetPersonTypeFromCuit(clienCuit),
                    ProductCode = productCode
                };

                var transactionResult = _itauService.CallItauApiCreateTransaction(registerCvuDto, cuitBu); // Solicita la creacion de la cvu-itau
                var cvuDto = CreateCvuDto(transactionResult, accountBalanceId); // Agrega el AccountBalaces a la tabla CvuEntity con status 0 

                return CreateCvu(cvuDto);
            }
            catch (Exception ex)
            {
                Serilog.Log.Error("Error: BeginCvuTransactionCreation: {@ex}", ex);
            }

            return false;
        }

        public void CvuMassCreationProcess()
        {
            //List<AccountBalanceDTO> accountBalances, List<ProductCodeBusinessUnitDTO> productCodesBusinessUnitDto
            var accountBalances = _accountBalanceRepository.GelAllAccountBalanceWithOutCVU();
            var productCodes = accountBalances.Select(it => it.Product).ToList();
            var productCodesBusinessUnitDto = _paymentService.GetBusinessUnitByProductCodes(productCodes);

            Serilog.Log.Information("Comenzo creacion masiva CVU para: {@a}", accountBalances);
            foreach (var accountBalance in accountBalances)
            {
                var businessUnitCuit =
                    productCodesBusinessUnitDto.FirstOrDefault(it => it.Codigo == accountBalance.Product)
                        ?.BusinessUnitCuit;
                if (string.IsNullOrEmpty(businessUnitCuit))
                {
                    Serilog.Log.Error("Error: BusinnesUnitCuit no existe para el AccountBalance: {@ab}",
                        accountBalance);
                    continue;
                }

                var cvuEntity = _cvuEntityRepository.GetSingle(
                    predicate: it => it.AccountBalance.Id == accountBalance.Id,
                    orderBy: null,
                    include: it => it.Include(c => c.AccountBalance));

                if (cvuEntity != null)
                    continue;

                var ret = BeginCvuTransactionCreation(accountBalance.Id, accountBalance.Product, businessUnitCuit, accountBalance.ClientCuit);
                if (ret)
                {
                    Serilog.Log.Information(
                        "Proceso alta de Cvu finalizado correctamente para el AccountBalance: {@ab}",
                        accountBalance);
                }
                else
                {
                    Serilog.Log.Error(
                        "Error: CvuMassCreationProcess: para el AccountBalance: {@ab}",
                        accountBalance);
                }

            }
        }

        public string GetCuitBuFromTransactionId(string transactionId)
        {
            var cvu = _cvuEntityRepository.GetSingle(
                predicate: it => it.ItauCreationTransactionId == transactionId,
                orderBy: null,
                include: it => it.Include(x => x.AccountBalance));
            var company = _companyRepository.GetByRazonSocial(cvu.AccountBalance.BusinessUnit);
            return company.Cuit;
        }

        public IEnumerable<CvuEntityDto> GetCvuEntitiesByIdAccounBalance(int IdAccountBalance)
        {
            var cvuEntities = _cvuEntityRepository.GetAll(x => x, y => y.AccountBalanceId == IdAccountBalance, orderBy: null, include: null, noTracking: true).ToList();

            return cvuEntities.Select(cvuEntityDto => new CvuEntityDto()
            {
                Id = cvuEntityDto.Id,
                ItauCreationTransactionId = cvuEntityDto.ItauCreationTransactionId,
                CvuValue = cvuEntityDto.CvuValue,
                Alias = cvuEntityDto.Alias,
                AccountBalanceId = cvuEntityDto.AccountBalanceId,
                Currency = cvuEntityDto.Currency
            });
        }

        public CvuEntity GetById(int cvuEntityId)
        {
            return _cvuEntityRepository.GetSingle(x => x.Id == cvuEntityId, null, I => I.Include(i => i.AccountBalance));
        }
    }
}
