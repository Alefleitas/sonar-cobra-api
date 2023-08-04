using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using Microsoft.Extensions.Options;
using Newtonsoft.Json;
using nordelta.cobra.webapi.Helpers;
using nordelta.cobra.webapi.Models;
using nordelta.cobra.webapi.Models.ValueObject.BankFiles;
using nordelta.cobra.webapi.Models.ValueObject.BankFiles.Constants;
using nordelta.cobra.webapi.Models.ValueObject.BankFiles.GaliciaFiles;
using nordelta.cobra.webapi.Models.ValueObject.BankFiles.ItauFiles;
using nordelta.cobra.webapi.Models.ValueObject.BankFiles.SantanderFiles;
using nordelta.cobra.webapi.Models.ValueObject.ItauPsp;
using nordelta.cobra.webapi.Services.Contracts;
using nordelta.cobra.webapi.Services.DTOs;
using nordelta.cobra.webapi.Utils;
using Serilog;

namespace nordelta.cobra.webapi.Services
{
    public class ProcessTransactionService : IProcessTransactionService
    {
        private readonly ICvuEntityService _cvuEntityService;
        private readonly IItauService _itauService;
        private readonly IPaymentMethodService _paymentMethodService;
        private readonly IPaymentDetailService _paymentDetailService;
        private readonly IPaymentService _paymentService;
        private readonly IValidacionClienteService _validacionClienteService;
        private readonly List<ItauPspItem> _itemPspIds;
        private readonly ICompanyService _companyService;

        public ProcessTransactionService(
            ICvuEntityService cvuEntityService,
            IItauService itauService,
            IPaymentMethodService paymentMethodService,
            IPaymentDetailService paymentDetailService,
            IOptionsMonitor<List<ItauPspItem>> itemPspConfig,
            IPaymentService paymentService,
            IValidacionClienteService validacionClienteService,
            ICompanyService companyService
            )
        {
            _itauService = itauService;
            _cvuEntityService = cvuEntityService;
            _paymentMethodService = paymentMethodService;
            _paymentDetailService = paymentDetailService;
            _paymentService = paymentService;
            _validacionClienteService = validacionClienteService;
            _itemPspIds = itemPspConfig.Get(ItauPspItem.ItauPspItems);
            _companyService = companyService;
        }



        public bool ProcessTransactionResult(TransactionResultDto transactionResult, string companySocialReason)
        {

            var company = _companyService.GetByRazonSocial(companySocialReason);
            if (company == null)
            {
                throw new Exception($"Internal Error. Company not found for this name: {companySocialReason}");
            }

            switch (transactionResult.TransactionType)
            {
                case TransactionType.operationTransaction:
                    if (transactionResult.Status.Code == TransactionStatusCodes.OkOperationCashIn)
                    {
                        var operatioInfo = _itauService.CallItauGetOperationInformation(transactionResult.Uri, company.Cuit);

                        if (operatioInfo is null)
                        {
                            Log.Information("Ocurrió un error al intentar procesar un pago CvuOperation por webhook, " +
                                "no se pudo obtener la OperationInfo para {@transactionResult} de itau", transactionResult);
                            return false;
                        }

                        // Se usa "LocalDateTime.GetDateTimeNow()" debido a que fechaNegocio puede ser el dia de mañana si el pago se realiza despues de las 18:00
                        operatioInfo.FechaNegocio = LocalDateTime.GetDateTimeNow().ToString("yyyy-MM-dd HH:mm:ss.fff");
                        operatioInfo.CodigoOrganismo = _itemPspIds.First(x => x.VendorCuit == company.Cuit && x.ProductoNumero == ItauPspItem.ServicioDeCvu).CodigoOrganismo;

                        var cvuOperation = _paymentMethodService.CreateCvuOperation(operatioInfo);

                        try
                        {

                            if (cvuOperation is null)
                            {
                                Log.Information("Ocurrió un error al intentar procesar un pago CvuOperation por webhook, " +
                                    "no se pudo crear CvuOperation: {@CvuOperation}", operatioInfo);
                                return false;
                            }
                            cvuOperation.Status = PaymentStatus.Finalized;

                            return _paymentMethodService.Update(cvuOperation) is not null;
                        }
                        catch (Exception ex)
                        {
                            Log.Error(ex, "Ocurrió un error al intentar informar pago {method} auotomaticamente " +
                                "Msg : {msg}" +
                                "PaymentMethod : {@paymentMethod}", cvuOperation.Instrument, ex.Message, cvuOperation);
                            return false;
                        }
                        finally
                        {
                            // INFORMAR PAGO
                            var accountBalance = _cvuEntityService.GetById(cvuOperation.CvuEntityId)?.AccountBalance;

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

                            _paymentService.InformPaymentMethodDone(cvuOperation, idClientOracle, idSiteClientOracle, cvuOperation.TransactionDate);

                        }
                    }
                    break;
                case TransactionType.createCvuTransaction:
                    var cvuInfo = _itauService.CallItauApiGetCvuInformation(transactionResult.Uri, company.Cuit);
                    if (cvuInfo != null)
                    {
                        return _cvuEntityService.CompleteCvuCreationForTransactionId(transactionResult.TransactionId, cvuInfo);
                    }
                    break;
            }
            return false;
        }

        public void ProcessRegistroFiles(IEnumerable<FileRegistro> fileRegistros)
        {
            var registros = fileRegistros.ToList();

            foreach (var registro in registros)
            {
                try
                {
                    switch (registro)
                    {
                        case PsPaymentItau cashIn:
                            {
                                var operatioInfo = new OperationInformationResultDto
                                {
                                    Amount = cashIn.Registro.Importe.ToString(CultureInfo.InvariantCulture),
                                    Cvu = new CvuDto
                                    {
                                        Cvu = cashIn.Registro.IdCvu,
                                        Cuit = cashIn.Registro.CuitCvu,
                                        Currency = Currency.ARS,
                                    },
                                    CoelsaId = cashIn.Registro.IdTransaccion,
                                    OperationId = cashIn.Registro.IdTransaccion,
                                    FechaNegocio = (cashIn.Registro.FechaOperacion.Date + DateTime.Now.TimeOfDay).ToString("yyyy-MM-dd HH:mm:ss.fff"),
                                    CodigoOrganismo = _itemPspIds.First(x => x.VendorCuit == cashIn.Header.Cuit &&
                                                                             x.ProductoNumero == ItauPspItem.ServicioDeCvu)?.CodigoOrganismo
                                };

                                if (_paymentMethodService.CreateCvuOperation(operatioInfo) is null)
                                    Log.Error("No se pudo conciliar la operación PS: {@Reg}", operatioInfo);
                            }
                            break;
                        case CvRegistroCvu cvRegistroCvu:
                            {
                                var cvuInfo = new TransactionResultDto
                                {
                                    Alias = cvRegistroCvu.AliasCuentaCvu,
                                    Cvu = cvRegistroCvu.CvuId
                                };
                                if (!_cvuEntityService.CompleteCvuCreationForTransactionId(cvRegistroCvu.TransaccionId, cvuInfo))
                                    Log.Error("No se pudo conciliar el Registro CV: @{@Reg}", cvuInfo);
                            }
                            break;
                        case PaymentItau payment:
                            {
                                ProcessPaymentOfItau(payment);
                            }
                            break;
                        case PaymentSantander payment:
                            {
                                ProcessPaymentOfSantander(payment);
                            }
                            break;
                        case PaymentGalicia payment:
                            {
                                ProcessPaymentOfGalicia(payment);
                            }
                            break;
                    }
                }
                catch (Exception ex)
                {
                    Log.Error(ex, "ProcessRegistroFiles: Ocurrió un error al momento de procesar un registro");
                }
            }
        }

        public bool ProcessPaymentOfItau(PaymentItau paymentItau)
        {
            try
            {
                var paymentDetails = new List<PaymentDetail>();
                var payment = paymentItau.Instruments.FirstOrDefault();
                var validStates = new List<string> { ItauFilesConstants.ACREDITADO,
                                                     ItauFilesConstants.ANULADO,
                                                     ItauFilesConstants.RECHAZADO,
                                                     ItauFilesConstants.REVERSADO,
                                                     ItauFilesConstants.RESCATADO};
                var rechazado = false;

                foreach (var instrument in paymentItau.Instruments)
                {
                    if (instrument.CodInstrumento == ItauFilesConstants.ECHEQ_AL_DIA || instrument.CodInstrumento == ItauFilesConstants.ECHEQ_CPD)
                    {
                        var paymentDetail = new PaymentDetail
                        {
                            SubOperationId = instrument.IdInstrumento.Trim(),
                            Amount = instrument.Importe.GetAmount(),
                            Status = instrument.CodEstado.Trim(),
                            Instrument = instrument.CodInstrumento.Trim(),
                            ErrorDetail = instrument.MotivoRechazo.Trim(),
                            CreditingDate = DateTime.ParseExact(instrument.FechaPago, "yyyyMMdd", CultureInfo.InvariantCulture),
                        };

                        if (instrument.CodInstrumento == ItauFilesConstants.ECHEQ_CPD && !string.IsNullOrEmpty(instrument.FechaDiferimiento.Trim()) &&
                            DateTime.ParseExact(instrument.FechaDiferimiento.Trim(), "yyyyMMdd", CultureInfo.InvariantCulture) <= LocalDateTime.GetDateTimeNow())
                        {
                            rechazado = true;
                        }

                        paymentDetails.Add(paymentDetail);
                    }
                }

                var totalAmount = paymentDetails.Select(x => x.Amount).Sum();
                var transactionDate = DateTime.ParseExact(payment.FechaPago, "yyyyMMdd", CultureInfo.InvariantCulture).Date + DateTime.Now.TimeOfDay;

                var echeqDto = new EcheqDto
                {
                    OperationId = payment.IdOperacion,
                    CuitCliente = payment.NroDocumento,
                    TransactionDate = transactionDate,
                    Amount = totalAmount,
                    Currency = Currency.ARS,
                    Source = PaymentSource.Itau,
                    Instrument = PaymentInstrument.ECHEQ,
                    OlapAcuerdo = _itemPspIds.First(x => x.VendorCuit == payment.CuitEmpresa &&
                                                         x.ProductoNumero == ItauPspItem.ServicioDeCobranzas)?.CodigoOrganismo
                };

                echeqDto.Status = rechazado ? PaymentStatus.Rejected : paymentDetails.All(x => validStates.Contains(x.Status)) ? PaymentStatus.InProcess : PaymentStatus.Pending;
                echeqDto.Amount = paymentDetails.Select(x => x.Amount).Sum();

                if (!_paymentMethodService.CreatePaymentMethod(echeqDto))
                {
                    Log.Error("ProcessPaymentOfItau: No se pudo conciliar la operación Echeq: {@echeqDto}", echeqDto);
                    return false;
                }

                // Buscamos el payment method que creamos
                var paymentMethod = _paymentMethodService.GetPaymentMethod(echeqDto.OperationId, echeqDto.Source, echeqDto.Instrument);
                var res = _paymentDetailService.GetAllByPaymentMethodId(paymentMethod.Id);

                paymentDetails.ForEach(x => x.PaymentMethodId = paymentMethod.Id);

                if (!res.Any())
                {
                    if (!_paymentDetailService.CreateAll(paymentDetails))
                    {
                        Log.Error("ProcessPaymentOfItau: Error al intentar crear payementDetails");
                    }
                }
                else
                {
                    if (!_paymentDetailService.UpdateAll(paymentDetails))
                    {
                        Log.Error("ProcessPaymentOfItau: Error al intentar actualizar payementDetails");
                    }
                }
            }
            catch (Exception ex)
            {
                Log.Error(ex, "Error al intenar procesar registros de itau");
                return false;
            }

            return true;
        }

        public void ProcessPaymentOfSantander(PaymentSantander paymentSantander)
        {
            try
            {
                Log.Information("ProcessPaymentOfSantander: Procesando pago santander... " +
                    "\n Pago: {@payment} " +
                    "\n Detalles Pago: {@paymentDetails}", paymentSantander.Payment, paymentSantander.Documents);

                var method = paymentSantander.Header.RazonSocialEmpresa.Trim().Split(' ')[1];
                var currency = (Currency)int.Parse(paymentSantander.Header.CodigoMoneda);
                var codigoOrganismo = paymentSantander.Header.CodigoOrganismo;
                var obsLibres = paymentSantander.Documents.Select(d => new
                {
                    ObsLibrePra = d.DatoLibre1.Trim().Split(' ')[0] + "|" + d.DatoLibre1.Trim().Split(' ')[1],
                    ObsLibreSda = d.TipoComprobante.Trim() == "PC" ? "|" + d.DatoLibre2.Trim() : d.DatoLibre2.Trim()
                });

                // CASH
                if (method == SantanderFilesConstants.EFECPESOS || method == SantanderFilesConstants.EFECUSD)
                {
                    var payment = new CashDto
                    {
                        CuitCliente = paymentSantander.Payment.CuitCliente.Trim(),
                        OperationId = paymentSantander.Payment.IdRegistro,
                        Amount = paymentSantander.Payment.TotalPagado.GetAmount(),
                        TransactionDate = DateTime.ParseExact(paymentSantander.Payment.FechaPago, "yyyyMMdd", CultureInfo.InvariantCulture),
                        Currency = currency,
                        Instrument = PaymentInstrument.CASH,
                        Source = PaymentSource.Santander,
                        Status = PaymentStatus.Pending,
                        ObsLibres = obsLibres,
                        OlapAcuerdo = codigoOrganismo
                    };

                    if (!_paymentMethodService.CreatePaymentMethod(payment))
                        Log.Information("ProcessPaymentOfSantander: No se pudo procesar correctamente pago de santander: {@payment}", payment);
                    else
                    {
                        // Agregar payment details
                        var paymentMethod = _paymentMethodService.GetPaymentMethod(payment.OperationId, payment.Source, payment.Instrument);

                        var paymentDetails = paymentSantander.Documents.Select(x => new PaymentDetail
                        {
                            SubOperationId = x.NroComprobante,
                            Amount = x.ImportePago.GetAmount(),
                            Status = "PAGADO", // Es el estado por defecto de los detalles de pago de santander, ya que los mismos fueron aprobados previamente
                            Instrument = payment.Instrument.ToString(),
                            CreditingDate = paymentMethod.TransactionDate,
                            PaymentMethodId = paymentMethod.Id
                        }).ToList();

                        if (!_paymentDetailService.CreateAll(paymentDetails))
                            Log.Information("ProcessPaymentOfSantander: No se pudieron guardar los detalles de pago : {@paymentDetails}", paymentSantander.Documents);
                    }
                }

                // CHEQUE
                if (method == SantanderFilesConstants.CHEQ)
                {
                    var payment = new ChequeDto
                    {
                        CuitCliente = paymentSantander.Payment.CuitCliente.Trim(),
                        OperationId = paymentSantander.Payment.IdRegistro,
                        Amount = paymentSantander.Payment.TotalPagado.GetAmount(),
                        TransactionDate = DateTime.ParseExact(paymentSantander.Payment.FechaPago, "yyyyMMdd", CultureInfo.InvariantCulture),
                        Currency = currency,
                        Instrument = PaymentInstrument.CHEQUE,
                        Source = PaymentSource.Santander,
                        Status = PaymentStatus.Pending,
                        ObsLibres = obsLibres,
                        OlapAcuerdo = codigoOrganismo
                    };

                    if (!_paymentMethodService.CreatePaymentMethod(payment))
                        Log.Information("ProcessPaymentOfSantander: No se pudo procesar correctamente pago de santander: {@payment}", payment);
                    else
                    {
                        // Agregar payment details
                        var paymentMethod = _paymentMethodService.GetPaymentMethod(payment.OperationId, payment.Source, payment.Instrument);

                        var paymentDetails = paymentSantander.Documents.Select(x => new PaymentDetail
                        {
                            SubOperationId = x.NroComprobante,
                            Amount = double.Parse(x.ImportePago),
                            Status = "PAGADO", // Es el estado por defecto de los detalles de pago de santander, ya que los mismos fueron aprobados previamente
                            Instrument = payment.Instrument.ToString(),
                            CreditingDate = paymentMethod.TransactionDate,
                            PaymentMethodId = paymentMethod.Id
                        }).ToList();

                        if (!_paymentDetailService.CreateAll(paymentDetails))
                            Log.Information("ProcessPaymentOfSantander: No se pudieron guardar los detalles de pago : {@paymentDetails}", paymentSantander.Documents);
                    }
                }
            }
            catch (Exception ex)
            {
                Log.Error(ex, "ProcessPaymentOfSantander: Ocurrió un error al intentar procesar pago santander - nroRendicion: {nroRendicion}", paymentSantander?.Header?.NroRendicion);
            }
        }

        public bool ProcessPaymentOfGalicia(PaymentGalicia payment)
        {
            try
            {
                var currency = (Currency)int.Parse(payment.Pago.Moneda);

                if (payment.Pago.FormaPago.Trim() != GaliciaFilesConstants.Efectivo)
                {
                    Log.Warning("ProcessPaymentOfGalicia: No se pudo procesar correctamente un pago de Galicia realizado por el cuit/cuil {cuit} debido a que la forma de pago no es en EFECTIVO", payment.Pago.Cuit);
                    return false;
                }

                if (payment.Pago.PagoAnulado.Trim() == GaliciaFilesConstants.Pago_Anulado)
                {
                    Log.Warning("ProcessPaymentOfGalicia: No se pudo procesar correctamente un pago de Galicia realizado por el cuit/cuil {cuit} debido a que el pago fue ANULADO", payment.Pago.Cuit);
                    return false;
                }

                var galiciaInfo = new CashDto
                {
                    OlapAcuerdo = payment.Header.IdEmpresa.Trim(),
                    CuitCliente = payment.Pago.Cuit.Trim(),
                    OperationId = payment.Pago.IdPago,
                    Amount = payment.Pago.ImportePago.GetAmount(),
                    TransactionDate = payment.Pago.FechaPago,
                    Currency = currency,
                    Instrument = PaymentInstrument.CASH,
                    Source = PaymentSource.Galicia,
                    ObsLibre = int.Parse(payment.Pago.IdDocumentoInterno)
                };

                if (!_paymentMethodService.CreatePaymentMethod(galiciaInfo))
                {
                    Log.Warning("ProcessPaymentOfGalicia: No se pudo procesar correctamente pago de Galicia: {@galiciaInfo}", galiciaInfo);
                    return false;
                }
            }
            catch (Exception ex)
            {
                Log.Error(ex, "ProcessPaymentOfGalicia: Ocurrió un error al intentar procesar pago Galicia - IdPago: {IdPago} del cliente cuit/cuil {cuit}", payment.Pago.IdPago, payment.Pago.Cuit);
                return false;
            }

            return true;
        }

    }
}
