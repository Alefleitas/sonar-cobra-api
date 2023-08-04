using AutoMapper;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using nordelta.cobra.webapi.Helpers;
using nordelta.cobra.webapi.Models;
using nordelta.cobra.webapi.Models.ArchivoDeuda;
using nordelta.cobra.webapi.Repositories.Contracts;
using nordelta.cobra.webapi.Services.Contracts;
using nordelta.cobra.webapi.Services.DTOs;
using nordelta.cobra.webapi.Utils;
using RestSharp;
using Serilog;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;

namespace nordelta.cobra.webapi.Services
{
    public class PaymentMethodService : IPaymentMethodService
    {
        private readonly ICvuEntityRepository _cvuEntityRepository;
        private readonly IArchivoDeudaRepository _archivoDeudaRepository;
        private readonly IUserRepository _userRepository;
        private readonly IPaymentService _paymentService;
        private readonly IPaymentReportRepository _paymentReportRepository;
        private readonly IMailService _mailService;
        private readonly IPaymentMethodRepository _paymentMethodRepository;
        private readonly IConfiguration _configuration;
        private readonly IMapper _mapper;
        private readonly IPaymentDetailService _paymentDetailService;
        private readonly IValidacionClienteService _validacionClienteService;
        

        public PaymentMethodService(
            ICvuEntityRepository cvuEntityRepository,
            IArchivoDeudaRepository archivoDeudaRepository,
            IUserRepository userRepository,
            IPaymentService paymentService,
            IPaymentReportRepository paymentReportRepository,
            IMailService mailService,
            IPaymentMethodRepository paymentMethodRepository,
            IConfiguration configuration,
            IMapper mapper,
            IPaymentDetailService paymentDetailService,
            IValidacionClienteService validacionClienteService
           
            )
        {
            _cvuEntityRepository = cvuEntityRepository;
            _archivoDeudaRepository = archivoDeudaRepository;
            _userRepository = userRepository;
            _paymentService = paymentService;
            _paymentReportRepository = paymentReportRepository;
            _mailService = mailService;
            _paymentMethodRepository = paymentMethodRepository;
            _configuration = configuration;
            _mapper = mapper;
            _paymentDetailService = paymentDetailService;
            _validacionClienteService = validacionClienteService;
         
        }

        public void SendEmailPaymenNotInformed(PaymentMethod paymentMethod)
        {
            try
            {
                var payer = _userRepository.GetUserById(paymentMethod.Payer.Id);

                // Enviar un mail a "Cuentas a Cobrar" para que procecen manualmente el pago
                var emails = _configuration.GetSection("ServiceConfiguration:RecipientsEmailPaymentReport").Get<List<string>>();

                var msg = $"Se registro un PAGO NO INFORMADO " +
                    $"\n - Cliente : {payer.FirstName} {payer.LastName}" +
                    $"\n - Cuit : {payer.Cuit} " +
                    $"\n - Email : {payer.Email} " +
                    $"\n - ID de pago {paymentMethod.OperationId}" +
                    $"\n - Moneda {paymentMethod.Currency}" +
                    $"\n - Monto {paymentMethod.Amount}" +
                    $"\n - Fecha : {paymentMethod.TransactionDate} " +
                    $"\n - Medio : {paymentMethod.Instrument} ";

                _mailService.SendNotificationEmail(emails, "PAGO NO INFORMADO", msg);

                Log.Information("SendEmailPaymenNotInformed: Se envio un mail por un pago no informado" +
                    "\n Destinatarios: {emails}" +
                    "\n Msg: {msg}" +
                    "\n PaymentMethod: {@paymentMethod}", emails, msg, paymentMethod);
            }
            catch (Exception ex)
            {
                Log.Error(ex, "SendEmailPaymenNotInformed: Ocurrió un error al intentar enviar un mail por un pago no informado" +
                    "\n PaymentMethod: {@paymentMethod}", paymentMethod);
            }
        }

       

        public void InformAllPaymentMethodDone()
        {
            var paymentMethods = _paymentMethodRepository.GetAll(
                    predicate: it => it.Status == PaymentStatus.InProcess &&
                                    (it.Instrument == PaymentInstrument.CvuOperation ||
                                     it.Instrument == PaymentInstrument.ECHEQ ||
                                     it.Instrument == PaymentInstrument.CASH ||
                                     it.Instrument == PaymentInstrument.CHEQUE),
                    orderBy: null,
                    noTracking: false);

            var milliSecondsCount = 1000;

            foreach (var paymentMethod in paymentMethods)
            {
                try
                {
                    switch (paymentMethod)
                    {
                        case CvuOperation cvuOperation:
                            {
                                var accountBalance = _cvuEntityRepository.GetSingle(x => x.Id == cvuOperation.CvuEntityId, null,
                                                                                    I => I.Include(i => i.AccountBalance))?.AccountBalance;

                                if (accountBalance is null)
                                {
                                    throw new Exception("No se encontró AccountBalance");
                                }

                                var validacionCliente = _validacionClienteService
                                    .GetByCuitClientAndProductCode(accountBalance.ClientCuit, accountBalance.Product);

                                if (validacionCliente is null)
                                {
                                    throw new Exception("No se encontró ValidacionCliente");
                                }

                                var isValidIdClientOracle = int.TryParse(validacionCliente.AccountNumber.Trim(), out int idClientOracle);
                                var isValidIdSiteClientOracle = int.TryParse(validacionCliente.SiteUseNumber.Trim(), out int idSiteClientOracle);

                                if (!(isValidIdClientOracle && isValidIdSiteClientOracle))
                                {
                                    throw new Exception("No se encontraron IdClienteOracle y IdSiteClienteOracle validos");
                                }

                                milliSecondsCount++;
                                var fechaRecibo = paymentMethod.TransactionDate.AddMilliseconds(milliSecondsCount);
                                _paymentService.InformPaymentMethodDone(paymentMethod, idClientOracle, idSiteClientOracle, fechaRecibo);
                            }
                            break;
                        case Echeq echeqOperation:
                            {
                                var paymentReport = _paymentReportRepository.GetSingle(x => x.PayerId == paymentMethod.Payer.Id &&
                                                                                            x.Status == PaymentReportStatus.Created &&
                                                                                            x.ReportDateVto > LocalDateTime.GetDateTimeNow());

                                if (paymentReport is null)
                                {
                                    paymentMethod.Status = PaymentStatus.InformedManually;
                                    _paymentMethodRepository.Update(paymentMethod);

                                    // Envio un mail por un pago no informado
                                    SendEmailPaymenNotInformed(paymentMethod);

                                    throw new Exception("No se encontró PaymentReport");
                                }

                                var validacionCliente = _validacionClienteService.GetByCuitClientAndProductCode(paymentReport.Cuit, paymentReport.Product);

                                if (validacionCliente is null)
                                {
                                    paymentMethod.Status = PaymentStatus.InformedManually;
                                    _paymentMethodRepository.Update(paymentMethod);

                                    // Envio un mail por un pago no informado
                                    SendEmailPaymenNotInformed(paymentMethod);

                                    throw new Exception("No se encontró ValidacionCliente");
                                }

                                var isValidIdClientOracle = int.TryParse(validacionCliente.AccountNumber.Trim(), out int idClientOracle);
                                var isValidIdSiteClientOracle = int.TryParse(validacionCliente.SiteUseNumber.Trim(), out int idSiteClientOracle);

                                if (!(isValidIdClientOracle && isValidIdSiteClientOracle))
                                {
                                    throw new Exception("No se encontraron IdClienteOracle y IdSiteClienteOracle validos");
                                }

                                milliSecondsCount++;
                                var fechaRecibo = paymentMethod.TransactionDate.AddMilliseconds(milliSecondsCount);
                                _paymentService.InformPaymentMethodDone(paymentMethod, idClientOracle, idSiteClientOracle, fechaRecibo);

                                paymentReport.Status = PaymentReportStatus.Processed;
                                _paymentReportRepository.Update(paymentReport);
                            }
                            break;
                        default: // Santander, Galicia
                            {
                                var detalleDeudas = _archivoDeudaRepository.GetDetallesDeudaByPaymentMethodId(paymentMethod.Id).OrderBy(x => x.FechaPrimerVenc);

                                if (!detalleDeudas.Any())
                                {
                                    throw new Exception("No se encontraron DetallesDeuda");
                                }

                                _paymentService.InformPaymentDone(detalleDeudas);
                            }
                            break;
                    }

                    paymentMethod.Status = PaymentStatus.Informing;
                    paymentMethod.InformedDate = LocalDateTime.GetDateTimeNow();
                    _paymentMethodRepository.Update(paymentMethod);
                }
                catch (Exception ex)
                {
                    Log.Error(ex, "Ocurrió un error al intentar informar pago {method} auotomaticamente " +
                        "\n Msg : {msg}" +
                        "\n PaymentMethod : {@paymentMethod}", paymentMethod.Instrument, ex.Message, paymentMethod);
                    continue;
                }
            }
        }

        public User GetPayer(string cuitClient)
        {
            try
            {
                var user = _userRepository.GetSsoUserByCuit(cuitClient);
                if (user == null)
                {
                    Log.Error("GetPayer: No se encontró usuario con el cuit {cuit}", cuitClient);
                    return null;
                }

                var payer = _userRepository.GetUserById(user.IdApplicationUser);
                if (payer == null)
                {
                    Log.Error("GetPayer: No se encontró payer con el Id {id} con el cuit {cuit}", user.IdApplicationUser, cuitClient);
                    return null;
                }

                return payer;
            }
            catch (Exception ex)
            {
                Log.Error(ex, "GetPayer: Ocurrió un error al intentar obtener un payer con cuit {cuit}", cuitClient);
            }

            return null;
        }

        public bool CreatePaymentMethod(PaymentMethodDto paymentMethodDto)
        {
            switch (paymentMethodDto)
            {
                case EcheqDto echeqDto:
                    return CreateOrUpdateEcheqOperation(echeqDto);
                case CashDto cashDto:
                    return CreateOrUpdateCashOperation(cashDto);
                case ChequeDto chequeDto:
                    return CreateOrUpdateChequeOperation(chequeDto);
            };

            return false;
        }

        public CvuOperation CreateCvuOperation(OperationInformationResultDto operationInformationResultDto)
        {
            try
            {
                Log.Information("Comienza el proceso de consiliación de Operación con OperationInformationResult {@operationInformationResultDto}", operationInformationResultDto);

                if (string.IsNullOrEmpty(operationInformationResultDto.FechaNegocio))
                {
                    Log.Error("Fecha Operacion no valida para la OperationInformationResult: {@operationInformationResultDto}", operationInformationResultDto);
                    return null;
                }

                var cvuEntity = _cvuEntityRepository.GetSingle(
                    predicate: entity => entity.CvuValue == operationInformationResultDto.Cvu.Cvu,
                    orderBy: null,
                    include: entities => entities.Include(it => it.AccountBalance));

                if (cvuEntity == null)
                {
                    Log.Error("CvuEntity no encontrada para la OperationInformationResult: {@operationInformationResultDto}", operationInformationResultDto);
                    return null;
                }

                var payer = GetPayer(cvuEntity.AccountBalance.ClientCuit);

                if (payer == null)
                {
                    Log.Error("User Payer no encontrado para AccountBalance.ClientId: {ClientId}", cvuEntity.AccountBalance.ClientId);
                    return null;
                }

                var cvuOperation = _paymentMethodRepository.GetSingleCvuOperation(x => x.Instrument == PaymentInstrument.CvuOperation &&
                        (x.OperationId == operationInformationResultDto.OperationId || x.CoelsaId == operationInformationResultDto.OperationId));

                if (cvuOperation != null)
                {
                    Log.Information($"La cvuOperation con OperationId: {operationInformationResultDto.OperationId} ya existe");
                    return null;
                }

                var newCvuOperation = new CvuOperation
                {
                    OlapAcuerdo = operationInformationResultDto.CodigoOrganismo,
                    Amount = operationInformationResultDto.Amount.GetDouble(),
                    CoelsaId = operationInformationResultDto.CoelsaId,
                    Currency = operationInformationResultDto.Cvu.Currency,
                    OperationId = operationInformationResultDto.OperationId,
                    TransactionDate = DateTime.Parse(operationInformationResultDto.FechaNegocio),
                    CvuEntityId = cvuEntity.Id,
                    Status = PaymentStatus.InProcess,
                    Source = PaymentSource.Itau,
                    Payer = payer
                };

                return _paymentMethodRepository.Insert(newCvuOperation) ? newCvuOperation : null;
            }
            catch (Exception ex)
            {
                Log.Error(ex, "CreateCvuOperation: Ocurrio un error al intentar registrar un pago por CVU {@cvu}", operationInformationResultDto);
            }

            return null;
        }

        public bool CreateOrUpdateEcheqOperation(EcheqDto echeqDto)
        {
            try
            {
                // Estados de ECHEQ: 
                // Pending:   El pago entro pero no esta en un estado valido para procesarlo
                // InProcess: El pago entro y esta en un estado para que sea informado a SGF > Oracle
                // Finalized: El pago fue informado a SGF > Oracle

                var echeqOperation = _paymentMethodRepository.GetSingle(p => p.OperationId == echeqDto.OperationId
                                                                          && p.Source == echeqDto.Source
                                                                          && p.Instrument == echeqDto.Instrument);

                if (echeqOperation != null && echeqOperation.Status != PaymentStatus.Pending && echeqOperation.Status != PaymentStatus.Rejected)
                {
                    Log.Information("CreateOrUpdateEcheqOperation: La operacion ECHEQ fue cancelada debido a que ya fue procesada: {@echeqDto}", echeqDto);
                    return false;
                }

                if (echeqOperation != null && (echeqOperation.Status == PaymentStatus.Pending || echeqOperation.Status == PaymentStatus.Rejected))
                {
                    echeqOperation.Status = echeqDto.Status;
                    echeqOperation.Amount = echeqDto.Amount;

                    return _paymentMethodRepository.Update(echeqOperation);
                }

                var payer = GetPayer(echeqDto.CuitCliente);

                if (payer == null)
                {
                    Log.Error("CreateOrUpdateEcheqOperation: No se encontró payer con el cuit {cuit} para la operación con OperationId {operationId} de origen {source}",
                        echeqDto.CuitCliente, echeqDto.OperationId, echeqDto.Source);
                    return false;
                }

                var newEcheq = new Echeq
                {
                    Payer = payer,
                    Amount = echeqDto.Amount,
                    Currency = echeqDto.Currency,
                    TransactionDate = echeqDto.TransactionDate,
                    OperationId = echeqDto.OperationId,
                    Status = echeqDto.Status,
                    Source = echeqDto.Source,
                    Type = echeqDto.Type,
                    OlapAcuerdo = echeqDto.OlapAcuerdo
                };

                return _paymentMethodRepository.Insert(newEcheq); ;
            }
            catch (Exception ex)
            {
                Log.Error(ex, "CreateEcheqOperation: Ocurrió un error al intentar registar un pago por ECHEQ {@echeq}", echeqDto);
            }

            return false;
        }

        public bool CreateOrUpdateCashOperation(CashDto cashDto)
        {
            try
            {
                var cashOperation = _paymentMethodRepository.GetSingle(p => p.OperationId == cashDto.OperationId
                                                                         && p.Source == cashDto.Source
                                                                         && p.Instrument == cashDto.Instrument);

                if (cashOperation != null && cashOperation.Status != PaymentStatus.Pending)
                {
                    Log.Information("CreateOrUpdateCashOperation: La operacion CASH fue cancelada debido a que ya fue procesada: {@cashDto}", cashDto);
                    return false;
                }

                if (cashOperation != null && cashOperation.Status == PaymentStatus.Pending)
                {
                    cashOperation.Status = cashDto.Status;
                    cashOperation.Amount = cashDto.Amount;

                    return _paymentMethodRepository.Update(cashOperation);
                }

                var payer = GetPayer(cashDto.CuitCliente);

                if (payer == null)
                {
                    Log.Error("CreateOrUpdateCashOperation: No se encontró payer con el cuit {cuit} para la operación con OperationId {operationId} de origen {source}",
                        cashDto.CuitCliente, cashDto.OperationId, cashDto.Source);
                    return false;
                }

                var newCash = new Cash()
                {
                    Payer = payer,
                    Amount = cashDto.Amount,
                    Currency = cashDto.Currency,
                    TransactionDate = cashDto.TransactionDate,
                    OperationId = cashDto.OperationId,
                    Instrument = cashDto.Instrument,
                    Source = cashDto.Source,
                    OlapAcuerdo = cashDto.OlapAcuerdo
                };

                var debts = new List<DetalleDeuda>();

                switch (newCash.Source)
                {
                    case PaymentSource.Santander:
                        {
                            foreach (var res in cashDto.ObsLibres)
                            {
                                var debt = _archivoDeudaRepository.GetByObsLibrePraAndObsLibreSda(res.ObsLibrePra, res.ObsLibreSda);
                                if (debt != null)
                                    debts.Add(debt);
                            }
                            newCash.Debts = debts;
                            newCash.Status = debts.Any() ? PaymentStatus.InProcess : PaymentStatus.Pending;
                        }
                        break;
                    case PaymentSource.Galicia:
                        {
                            var debt = _archivoDeudaRepository.GetLastDetalleDeudas().FirstOrDefault(d => d.Id == cashDto.ObsLibre);
                            if (debt != null)
                                debts.Add(debt);

                            newCash.Debts = debts;
                            newCash.Status = debts.Any() ? PaymentStatus.InProcess : PaymentStatus.Pending;
                        }
                        break;
                }

                return _paymentMethodRepository.Insert(newCash);
            }
            catch (Exception ex)
            {
                Log.Error(ex, "CreateOrUpdateCashOperation: Ocurrió un error al intentar registrar un pago por CASH {@cash}", cashDto);
            }

            return false;
        }

        public bool CreateOrUpdateChequeOperation(ChequeDto chequeDto)
        {
            try
            {
                var chequeOperation = _paymentMethodRepository.GetSingle(p => p.OperationId == chequeDto.OperationId
                                                                     && p.Source == chequeDto.Source
                                                                     && p.Instrument == chequeDto.Instrument);

                if (chequeOperation != null && chequeOperation.Status != PaymentStatus.Pending)
                {
                    Log.Information("CreateOrUpdateChequeOperation: La operacion CHEQUE fue cancelada debido a que ya fue procesada: {@chequeDto}", chequeDto);
                    return false;
                }

                if (chequeOperation != null && chequeOperation.Status == PaymentStatus.Pending)
                {
                    chequeOperation.Status = chequeDto.Status;
                    chequeOperation.Amount = chequeDto.Amount;

                    return _paymentMethodRepository.Update(chequeOperation);
                }

                var payer = GetPayer(chequeDto.CuitCliente);

                if (payer == null)
                {
                    Log.Error("CreateOrUpdateChequeOperation: No se encontró payer con el cuit {cuit} para la operación con OperationId {operationId} de origen {source}",
                        chequeDto.CuitCliente, chequeDto.OperationId, chequeDto.Source);
                    return false;
                }

                var newCash = new Cheque()
                {
                    Payer = payer,
                    Amount = chequeDto.Amount,
                    Currency = chequeDto.Currency,
                    TransactionDate = chequeDto.TransactionDate,
                    OperationId = chequeDto.OperationId,
                    Source = chequeDto.Source,
                    Type = chequeDto.Type,
                    OlapAcuerdo = chequeDto.OlapAcuerdo
                };

                var debts = new List<DetalleDeuda>();

                switch (newCash.Source)
                {
                    case PaymentSource.Santander:
                        {
                            foreach (var res in chequeDto.ObsLibres)
                            {
                                var debt = _archivoDeudaRepository.GetByObsLibrePraAndObsLibreSda(res.ObsLibrePra, res.ObsLibreSda);
                                if (debt != null)
                                    debts.Add(debt);
                            }
                            newCash.Debts = debts;
                            newCash.Status = debts.Any() ? PaymentStatus.InProcess : PaymentStatus.Pending;
                        }
                        break;
                }

                return _paymentMethodRepository.Insert(newCash);
            }
            catch (Exception ex)
            {
                Log.Error(ex, "CreateChequeOperation: Ocurrió un error al intentar registrar un pago por CHEQUE {@cheque}", chequeDto);
            }

            return false;
        }

        public List<PaymentMethodDto> GetPaymentMethods(PaymentInstrument? instrument = null)
        {
            var paymentMethodDtos = new List<PaymentMethodDto>();
            try
            {
                var paymentMethods = instrument == null ? _paymentMethodRepository.GetAll(noTracking: false).ToList()
                    : _paymentMethodRepository.GetAll(noTracking: false, predicate: x => x.Instrument == instrument).ToList();

                if (paymentMethods.Any())
                {
                    paymentMethods.ForEach(x => x.Payer = x != null ? _userRepository.GetUserById(x.Payer.Id) : new User());
                    paymentMethodDtos = _mapper.Map<IEnumerable<PaymentMethod>, IEnumerable<PaymentMethodDto>>(paymentMethods).ToList();
                    paymentMethodDtos.ForEach(x => x.HasPaymentDetail = _paymentDetailService.HasPaymentDetail(x.Id));
                }
            }
            catch (Exception ex)
            {
                Log.Error(ex, "GetPaymentMethods: Ocurrió un error al intentar obtener todos los metodos de pago");
            }

            return paymentMethodDtos;
        }

        public PaymentMethod GetPaymentMethod(string operationId, PaymentSource source, PaymentInstrument instrument)
        {
            try
            {
                return _paymentMethodRepository.GetSingle(x => x.OperationId == operationId && x.Source == source && x.Instrument == instrument);
            }
            catch (Exception ex)
            {
                Log.Error(ex, "GetPaymentMethod: Ocurrió un error al intenar obtener PaymentMethod");
            }

            return null;
        }

        public List<User> GetAllUsersFromDebin()
        {
            var paymentMethods = _paymentMethodRepository.GetAll(noTracking: false, predicate: x => x.Instrument == PaymentInstrument.DEBIN)
                                                         .GroupBy(x => x.Payer.Id)
                                                         .Select(x => x.First())
                                                         .ToList();
            var usersList = new List<User>();

            if (paymentMethods.Any())
            {
                paymentMethods.ForEach(x =>
                {
                    var test = _userRepository.GetUserById(x.Payer.Id);
                    usersList.Add(_userRepository.GetUserById(x.Payer.Id));
                });
            }
            return usersList;
        }

        public InformDebinPagination GetDebinesWithPagination(int pageSize, int pageNumber, string payerId, string fechaDesde, string fechaHasta)
        {
            var debines = new InformDebinPagination();

            try
            {
                List<PaymentMethod> paymentMethods = _paymentMethodRepository.GetAll(noTracking: false, predicate: x => x.Instrument == PaymentInstrument.DEBIN);
                IQueryable<PaymentMethod> queryable = paymentMethods.AsQueryable();

                if (!string.IsNullOrWhiteSpace(payerId))
                {
                    queryable = queryable.Where(x => x.Payer.Id == payerId);
                }
                if (!string.IsNullOrWhiteSpace(fechaDesde))
                {
                    var desde = DateTime.ParseExact(fechaDesde, "dd/MM/yyyy", CultureInfo.InvariantCulture).Date;
                    queryable = queryable.Where(x => x.TransactionDate.Date >= desde);
                }
                if (!string.IsNullOrWhiteSpace(fechaHasta))
                {
                    var hasta = DateTime.ParseExact(fechaHasta, "dd/MM/yyyy", CultureInfo.InvariantCulture).Date;
                    queryable = queryable.Where(x => x.TransactionDate.Date <= hasta);
                }

                debines.TotalCount = queryable.Count();
                var results = queryable.Skip(pageSize * (pageNumber - 1)).Take(pageSize).ToList();

                if (results.Any())
                {
                    results.ForEach(x => x.Payer = _userRepository.GetUserById(x.Payer.Id));
                    debines.DebinList = _mapper.Map<IEnumerable<PaymentMethod>, IEnumerable<PaymentMethodDto>>(results).ToList();
                }

            }
            catch (Exception ex)
            {
                Log.Error("GetDebinesWithPagination: No se pudo obtener la lista de debines. Exception Detail: {@ex}", ex);
            }

            return debines;

        }

        public PaymentMethod Update(PaymentMethod paymentMethod)
        {
            return _paymentMethodRepository.Update(paymentMethod) ? paymentMethod : null;
        }

        public bool UpdatePaymentInformStatus(PaymentInformedDto paymentInformDto)
        {
            try
            {
                Log.Information("Se procesa resultado de pago informado a SGF" +
                    "\n paymentInformDto: {@paymentInformViewModel}", paymentInformDto);

                var paymenytMethod = _paymentMethodRepository.GetSingle(x => x.Id == paymentInformDto.PaymentMethodId);

                if (paymenytMethod is null)
                {
                    throw new Exception("No se econtró paymentMethod");
                }

                if (paymentInformDto.Result == PaymentInformedResult.LockBox_Generado)
                {
                    paymenytMethod.LockboxName = paymentInformDto.LockboxId;

                    if (_paymentMethodRepository.Update(paymenytMethod))
                    {
                        Log.Information("Se genero correctamente el archivo lockbox {lockbox}" +
                            "\n payment {@payment}" +
                            "\n paymentInformDto {@paymentInformDto}", paymentInformDto.LockboxId, paymenytMethod, paymentInformDto);
                        return true;
                    }
                    else
                    {
                        throw new Exception("No se pudo actualizar estado del pago");
                    }
                }
                else
                {
                    paymenytMethod.Status = PaymentStatus.ErrorInform;
                    Log.Information("No se genero el archivo Lockbox en SGF" +
                        "\n payment {@payment}" +
                        "\n paymentInformDto: {@paymentInformViewModel}", paymenytMethod, paymentInformDto);
                    return false;
                }
            }
            catch (Exception ex)
            {
                Log.Error(ex, "Ocurrió un error al momento de actualizar estado de pago informado" +
                    "\n msg: {msg}" +
                    "\n paymentInformDto: {@paymentInformDto}", ex.Message, paymentInformDto);
                throw;
            }
        }

        public void CheckAndFinalizePaymentInformed()
        {
            Log.Information("Comienza proceso para finalizar pagos en estado LockboxGenerated...");
            var paymentMethods = _paymentMethodRepository.GetAll(x => x.Status == PaymentStatus.Informing);

            foreach (var paymentMethod in paymentMethods)
            {
                try
                {
                    Log.Information("Procesando pago con Id {PaymentMethodId}...", paymentMethod.Id);
                    var tiempoDemorado = LocalDateTime.GetDateTimeNow() - (DateTime)paymentMethod.InformedDate;

                    var tiempoEsperaLockBox = TimeSpan.FromMinutes(_configuration.GetSection("TimeoutGenerateLockboxInMinutes").Get<int>());
                    var tiempoEsperaInformToOracle = TimeSpan.FromMinutes(_configuration.GetSection("TimeoutToInformOracleInMinutes").Get<int>());

                    if (paymentMethod.LockboxName is null)
                    {
                        if (tiempoDemorado.TotalMinutes >= tiempoEsperaLockBox.TotalMinutes)
                        {
                            Log.Information("Se supero el tiempo de espera para generar el lockbox de un pago informado" +
                                "\n payment: {@payment}", paymentMethod);

                            paymentMethod.Status = PaymentStatus.ErrorInform;
                            _paymentMethodRepository.Update(paymentMethod);
                        }
                        continue;
                    }
                    else if (tiempoDemorado.TotalMinutes >= tiempoEsperaInformToOracle.TotalMinutes)
                    {
                        Log.Information("Se supero el tiempo de espera para finalizar un pago informado" +
                            "\n payemnt: {@payment}", paymentMethod);

                        paymentMethod.Status = PaymentStatus.ErrorInform;
                        _paymentMethodRepository.Update(paymentMethod);
                        continue;
                    }

                    var logs = _paymentService.GetLogFromMiddleware(queryParams: $"?file={paymentMethod.LockboxName}");

                    var log = logs.FirstOrDefault(x => x.Description.ToUpper().Contains("LOCKBOX PROCESS WAS SUCCESSFUL"));
                    if (log is not null)
                    {
                        Log.Information("Se informo correctamente un pago a Oracle" +
                            "\n log: {@log}" +
                            "\n paymenteMethod: {@payment}", log, paymentMethod);
                        paymentMethod.Status = PaymentStatus.Finalized;
                        _paymentMethodRepository.Update(paymentMethod);
                        continue;
                    }

                    log = logs.FirstOrDefault(x => x.Description.ToUpper().Contains("LOCKBOX PROCESS WAS NOT SUCCESSFUL"));
                    if (log is not null)
                    {
                        Log.Information("Ocurrió un error al intentar informar un pago a Oracle" +
                            "\n log: {@log}" +
                            "\n paymenteMethod: {@payment}", log, paymentMethod);
                        paymentMethod.Status = PaymentStatus.ErrorInform;
                        _paymentMethodRepository.Update(paymentMethod);
                        continue;
                    }
                }
                catch (Exception ex)
                {
                    Log.Error(ex, "Error al finalizar un pago " +
                        "\n msg: {msg}" +
                        "\n paymentMethod: {@payment}", ex.Message, paymentMethod);
                }
            }
        }
    }
}
