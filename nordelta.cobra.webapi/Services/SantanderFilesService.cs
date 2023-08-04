using FileHelpers.MasterDetail;
using nordelta.cobra.webapi.Models.ArchivoDeuda;
using nordelta.cobra.webapi.Models;
using nordelta.cobra.webapi.Models.ValueObject.BankFiles.Constants;
using nordelta.cobra.webapi.Models.ValueObject.BankFiles.SantanderFiles;
using nordelta.cobra.webapi.Services.Contracts;
using nordelta.cobra.webapi.Utils;
using Serilog;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Extensions.Options;
using nordelta.cobra.webapi.Configuration;
using nordelta.cobra.webapi.Repositories.Contracts;

namespace nordelta.cobra.webapi.Services
{
    public class SantanderFilesService : ISantanderFilesService
    {
        private readonly IProcessTransactionService _processTransactionService;
        private readonly IArchivoDeudaRepository _archivoDeudaRepository;
        private readonly IPublishedDebtBankFileRepository _publishedDebtBankFileRepository;
        private readonly List<PublishDebtConfiguration> santanderBusinessUnits;

        public SantanderFilesService(
            IProcessTransactionService processTransactionService,
            IArchivoDeudaRepository archivoDeudaRepository,
            IPublishedDebtBankFileRepository publishedDebtBankFileRepository,
            IOptionsMonitor<List<PublishDebtConfiguration>> publishDebtConfig
            )
        {
            _processTransactionService = processTransactionService;
            _archivoDeudaRepository = archivoDeudaRepository;
            _publishedDebtBankFileRepository = publishedDebtBankFileRepository;
            santanderBusinessUnits = publishDebtConfig.Get(PublishDebtConfiguration.SantanderBUConfig);
        }

        private bool CreateRecordsFiles(FileInfo fileInfo)
        {
            // Necessary folders for Santander files
            var folderPath = AppDomain.CurrentDomain.BaseDirectory;
            foreach (var nameFolder in new List<string> { "SantanderFiles", "IN", "Process" })
            {
                folderPath = Path.Combine(folderPath, nameFolder);
                if (!Directory.Exists(folderPath))
                    Directory.CreateDirectory(folderPath);
            }

            foreach (var file in new DirectoryInfo(folderPath).GetFiles())
                file.Delete();

            try
            {
                var swPR = new StreamWriter(Path.Combine(folderPath, SantanderFilesConstants.PR + fileInfo.Name));
                var swPI = new StreamWriter(Path.Combine(folderPath, SantanderFilesConstants.PI + fileInfo.Name));
                var swPD = new StreamWriter(Path.Combine(folderPath, SantanderFilesConstants.PD + fileInfo.Name));

                using TextReader tr = new StreamReader(fileInfo.Open(FileMode.Open));
                {
                    var line = tr.ReadLine();

                    while (!string.IsNullOrEmpty(line) && line.Length > 42)
                    {
                        var typeLine = line[22].ToString() + line[42].ToString();

                        switch (typeLine)
                        {
                            case SantanderFilesConstants.TypeHeader:
                                swPR.WriteLine(line);
                                swPI.WriteLine(line);
                                swPD.WriteLine(line);
                                break;
                            case SantanderFilesConstants.TypePR:      // Pago Registro
                                swPR.WriteLine(line);
                                break;
                            case SantanderFilesConstants.TypePI:      // Instrumento Pago
                                swPI.WriteLine(line);
                                break;
                            case SantanderFilesConstants.TypePD:      // Documento Pago
                                swPD.WriteLine(line);
                                break;
                            case SantanderFilesConstants.TypeTrailer: 
                                swPR.WriteLine(line);
                                swPI.WriteLine(line);
                                swPD.WriteLine(line);
                                break;
                        }
                        line = tr.ReadLine();
                    }
                };

                swPR.Close();
                swPI.Close();
                swPD.Close();
            }
            catch (Exception ex)
            {
                Log.Error(ex, "CreateRecordsFiles: Ocurrió un error al intentar crear registros a partir del archivo {nameFile} de Santander", fileInfo.Name);
            }

            return new DirectoryInfo(folderPath).GetFiles().ToList().Count() == 3;
        }

        private RecordAction SelectorMasterOrDetail(string record, int index, string masterToken)
        {
            if (record.Length < 2 || record[index].Equals('T'))
                return RecordAction.Skip;

            return record[index].Equals(masterToken.ToCharArray()[0]) ? RecordAction.Master : RecordAction.Detail;
        }

        private Dictionary<string, dynamic> GetTypesOfRecordsAndTheirDetails()
        {
            // Necessary folders for Santander files
            var folderPath = AppDomain.CurrentDomain.BaseDirectory;
            foreach (var nameFolder in new List<string> { "SantanderFiles", "IN", "Process" })
            {
                folderPath = Path.Combine(folderPath, nameFolder);
                if (!Directory.Exists(folderPath))
                    Directory.CreateDirectory(folderPath);
            }

            var ret = new Dictionary<string, dynamic>();
            var files = new DirectoryInfo(folderPath).GetFiles("*.txt");

            foreach (var file in files)
            {
                try
                {
                    using TextReader tr = new StreamReader(file.Open(FileMode.Open));
                    {
                        if (file.Name.StartsWith(SantanderFilesConstants.PR))
                        {
                            var list = new List<PrSantander>();
                            var engine = new MasterDetailEngine<HeaderSantander, PrSantander>(r =>
                                SelectorMasterOrDetail(r, SantanderFilesConstants.IndexTR, SantanderFilesConstants.Header));

                            var res = engine.ReadStream(tr);
                            foreach (var group in res)
                            {
                                list.AddRange(@group.Details);
                            }

                            ret.Add(SantanderFilesConstants.PR, list);
                            ret.Add(SantanderFilesConstants.Header, res.FirstOrDefault().Master);
                            continue;
                        }

                        if (file.Name.StartsWith(SantanderFilesConstants.PI))
                        {
                            var list = new List<PiSantander>();
                            var engine = new MasterDetailEngine<HeaderSantander, PiSantander>(r =>
                                SelectorMasterOrDetail(r, SantanderFilesConstants.IndexTR, SantanderFilesConstants.Header));

                            var res = engine.ReadStream(tr);
                            foreach (var group in res)
                            {
                                list.AddRange(@group.Details);
                            }

                            ret.Add(SantanderFilesConstants.PI, list);
                            continue;
                        }

                        if (file.Name.StartsWith(SantanderFilesConstants.PD))
                        {
                            var list = new List<PdSantander>();
                            var engine = new MasterDetailEngine<HeaderSantander, PdSantander>(r =>
                                SelectorMasterOrDetail(r, SantanderFilesConstants.IndexTR, SantanderFilesConstants.Header));

                            var res = engine.ReadStream(tr);
                            foreach (var group in res)
                            {
                                list.AddRange(@group.Details);
                            }

                            ret.Add(SantanderFilesConstants.PD, list);
                            continue;
                        }
                    }
                }
                catch (Exception ex)
                {
                    Log.Error(ex, "GetTypesOfRecordsAndTheirDetails: Ocurrió un error al intentar obtener los detalles del registro: {fileName}", file.Name);
                }
            }

            return ret;
        }

        private List<PaymentSantander> GetSantanderPayments(Dictionary<string, dynamic> recordTypes)
        {
            var santanderPayments = new List<PaymentSantander>();

            try
            {
                var header = (HeaderSantander)recordTypes[SantanderFilesConstants.Header];
                var payments = (List<PrSantander>)recordTypes[SantanderFilesConstants.PR];
                var documents = (List<PdSantander>)recordTypes[SantanderFilesConstants.PD];
                var instruments = (List<PiSantander>)recordTypes[SantanderFilesConstants.PI];

                foreach (var payment in payments)
                {
                    var rendicionDetail = new PaymentSantander
                    {
                        Header = header,
                        Payment = payment,
                        Documents = documents.Where(x => x.IdRegistro == payment.IdRegistro),
                        Instruments = instruments.Where(x => x.IdRegistro == payment.IdRegistro)
                    };

                    santanderPayments.Add(rendicionDetail);
                }
            }
            catch (Exception ex)
            {
                Log.Error(ex, "GetSantanderPayments: Ocurrió un error al intentar obtener pagos Santander a partir de regitros");
            }

            return santanderPayments;
        }

        public void GetAndProcessPaymentFilesOfSantander()
        {
            string folderPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "SantanderFiles");
            if (!Directory.Exists(folderPath))
                Directory.CreateDirectory(folderPath);

            var files = new DirectoryInfo(folderPath).GetFiles("*.txt");
            foreach (var file in files)
            {
                try
                {
                    if (CreateRecordsFiles(file))
                    {
                        var recordTypes = GetTypesOfRecordsAndTheirDetails();
                        var santanderPayments = GetSantanderPayments(recordTypes);

                        _processTransactionService.ProcessRegistroFiles(santanderPayments);
                    }
                    else
                    {
                        Log.Warning("CreateRecordsFiles: No se pudieron crear registros usando el archivo {filename} de Santander", file.Name);
                    }
                }
                catch (Exception ex)
                {
                    Log.Error(ex, "GetAndProcessPaymentFilesOfSantander: Ocurrio un error al intentar procesar archivo {fileName} de Santander", file.Name);
                }
            }
        }

        private List<string> CreateDetails(List<DetalleDeuda> detallesDeuda)
        {
            List<string> detailLines = new List<string>();
            StringBuilder detailBuilder = new StringBuilder();

            foreach (var detalleDeuda in detallesDeuda)
            {
                try
                {
                    detailBuilder.Append(detalleDeuda.TipoRegistro);
                    detailBuilder.Append(detalleDeuda.TipoOperacion);
                    detailBuilder.Append(detalleDeuda.CodigoMoneda);
                    detailBuilder.Append(detalleDeuda.NroCuitCliente.PadRight(15, ' ')); // Número de Cliente
                    detailBuilder.Append(detalleDeuda.TipoComprobante);
                    detailBuilder.Append(detalleDeuda.NroComprobante.PadRight(15, ' '));
                    detailBuilder.Append(detalleDeuda.NroCuota);
                    detailBuilder.Append(detalleDeuda.NombreCliente.PadRight(30, ' '));
                    detailBuilder.Append(detalleDeuda.DireccionCliente.PadRight(30, ' '));
                    detailBuilder.Append(detalleDeuda.DescripcionLocalidad.PadRight(20, ' '));
                    detailBuilder.Append(detalleDeuda.PrefijoCodPostal);
                    detailBuilder.Append(detalleDeuda.NroCodPostal.PadLeft(5, '0'));
                    detailBuilder.Append(detalleDeuda.UbicManzanaCodPostal.PadRight(4, ' '));
                    detailBuilder.Append(detalleDeuda.FechaPrimerVenc.PadRight(8, ' '));
                    detailBuilder.Append(detalleDeuda.ImportePrimerVenc.PadLeft(15, '0'));
                    detailBuilder.Append(detalleDeuda.FechaSegundoVenc.PadRight(8, ' '));
                    detailBuilder.Append(detalleDeuda.ImporteSegundoVenc.PadLeft(15, '0'));
                    detailBuilder.Append(detalleDeuda.FechaHastaDescuento.PadRight(8, ' '));
                    detailBuilder.Append(detalleDeuda.ImporteProntoPago.PadLeft(15, '0'));
                    detailBuilder.Append(detalleDeuda.FechaHastaPunitorios.PadRight(8, ' '));
                    detailBuilder.Append(detalleDeuda.TasaPunitorios.PadLeft(6, '0'));
                    detailBuilder.Append(detalleDeuda.MarcaExcepcionCobroComisionDepositante);
                    detailBuilder.Append(detalleDeuda.FormasCobroPermitidas.PadRight(10, ' '));
                    detailBuilder.Append(detalleDeuda.NroCuitCliente);
                    detailBuilder.Append(detalleDeuda.CodIngresosBrutos);
                    detailBuilder.Append(detalleDeuda.CodCondicionIva);
                    detailBuilder.Append(detalleDeuda.CodConcepto.PadRight(3, ' '));
                    detailBuilder.Append(detalleDeuda.DescCodigo.PadRight(40, ' '));
                    detailBuilder.Append(detalleDeuda.ObsLibrePrimera.PadRight(18, ' '));
                    detailBuilder.Append(detalleDeuda.ObsLibreSegunda.PadRight(15, ' '));
                    detailBuilder.Append(detalleDeuda.ObsLibreTercera.PadLeft(15, '0'));
                    detailBuilder.Append(detalleDeuda.ObsLibreCuarta.PadRight(80, ' '));
                    detailBuilder.Append("".PadLeft(243, ' ')); // Relleno

                    detailLines.Add(detailBuilder.ToString());
                    detailBuilder.Clear();
                }
                catch (Exception ex)
                {
                    Log.Error(ex, "CreateDetails: Ocurrió un error al intentar procesar detalle deuda con Id: {detalleDeudaId}", detalleDeuda.Id);
                    continue;
                }
            }

            Log.Debug("CreateDetails: Se procesaron correctamente {numDetailLines} de {numDetailDebts} detalles deuda", detailLines.Count(), detallesDeuda.Count());

            return detailLines;
        }

        public async Task CreateAndPublishDebtFilesToSantanderAsync()
        {
            // Necessary folders for Santander files
            var folderPath = AppDomain.CurrentDomain.BaseDirectory;
            foreach (var nameFolder in new List<string> { "SantanderFiles", "OUT" })
            {
                folderPath = Path.Combine(folderPath, nameFolder);
                if (!Directory.Exists(folderPath))
                    Directory.CreateDirectory(folderPath);
            }

            var builder = new StringBuilder();
            var lines = new List<string>();

            foreach (var BU in santanderBusinessUnits)
            {
                foreach (var acuerdo in BU.Acuerdos)
                {
                    try
                    {
                        var detalleDeudas = _archivoDeudaRepository.GetLastDetalleDeudasByCurrencyAndCuitEmpresa(acuerdo.Currency, BU.Cuit);

                        if (!detalleDeudas.Any())
                            continue;

                        var archivoDeuda = detalleDeudas.GroupBy(x => x.ArchivoDeudaId, (x, y) => y.FirstOrDefault().ArchivoDeuda).FirstOrDefault();

                        var publishedDebt = _publishedDebtBankFileRepository.GetSingle(predicate: x => x.CodigoOrganismo == acuerdo.CodigoOrganismo
                                                                                                            && x.Bank == PaymentSource.Santander,
                                                                                               orderBy: x => x.OrderByDescending(a => a.NroArchivo));

                        if (publishedDebt != null && publishedDebt.ArchivoDeudaId == archivoDeuda.Id)
                            continue;

                        var nroArchivo = publishedDebt == null ? acuerdo.NroArchivo
                                       : publishedDebt.NroArchivo < acuerdo.NroArchivo ? acuerdo.NroArchivo
                                       : publishedDebt.NroArchivo + 1;

                        var dateNow = DateTime.Now;
                        var dateString = dateNow.ToString("yyyyMMdd");
                        var timeString = dateNow.ToString("hhmmss");

                        // HEADER
                        builder.Append("H");
                        builder.Append(acuerdo.CodigoOrganismo);
                        builder.Append("007"); // Código de canal
                        builder.Append(nroArchivo.ToString().PadLeft(5, '0')); // Número de envío
                        builder.Append("00000"); // Última rendición procesada
                        builder.Append("N"); // Marca de actualización de cuenta comercial: S, P o N
                        builder.Append("O"); // Marca para publicación ON-LINE: O o B
                        builder.Append("".PadLeft(617, ' ')); // Filler

                        lines.Add(builder.ToString());
                        builder.Clear();

                        // DETAILS
                        var detailLines = CreateDetails(detalleDeudas);
                        lines.AddRange(detailLines);

                        //TRAILER
                        builder.Append("T");

                        var importeTotalPrimerVenc = detalleDeudas.Where(d => d.TipoOperacion.Trim().Equals("A")).Select(x => Convert.ToInt64(x.ImportePrimerVenc)).Sum();

                        builder.Append(importeTotalPrimerVenc.ToString().PadLeft(15, '0')); // (15) Importe total a 1er. Vencimiento: Sumatoria de Importes de cobro al 1er. Vencimiento. Cuando son bajas no suman en el importe.
                        builder.Append("".PadLeft(15, '0')); // Ceros
                        builder.Append(detailLines.Count.ToString().PadLeft(7, '0')); // (7) Cantidad de registros.
                        builder.Append("".PadLeft(612, ' ')); // Relleno

                        lines.Add(builder.ToString());
                        builder.Clear();

                        // CreateFile
                        var filePath = Path.Combine(folderPath, archivoDeuda.FormatedFileName + "_" + dateString + timeString + ".txt");
                        File.AppendAllLines(filePath, lines.ToArray(), Encoding.ASCII);

                        var newSentFile = new PublishedDebtBankFile
                        {
                            ArchivoDeudaId = archivoDeuda.Id,
                            CuitEmpresa = BU.Cuit,
                            CodigoOrganismo = acuerdo.CodigoOrganismo,
                            NroArchivo = nroArchivo,
                            FileName = archivoDeuda.FileName,
                            Bank = PaymentSource.Santander,
                            CreatedOn = LocalDateTime.GetDateTimeNow()
                        };

                        _publishedDebtBankFileRepository.Insert(newSentFile);

                        await Task.Delay(1000);
                    }
                    catch (Exception ex)
                    {
                        Log.Error(ex, "CreateDebtFilesForSantander: Ocurrió un error al intentar crear un archivo de publicacion deuda para Santander de {@BU}", BU);
                    }
                }
            }
        }
    }
}
