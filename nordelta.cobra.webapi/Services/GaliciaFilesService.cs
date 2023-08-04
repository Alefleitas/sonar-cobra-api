using FileHelpers.MasterDetail;
using Microsoft.Extensions.Options;
using nordelta.cobra.webapi.Configuration;
using nordelta.cobra.webapi.Helpers;
using nordelta.cobra.webapi.Models;
using nordelta.cobra.webapi.Models.ArchivoDeuda;
using nordelta.cobra.webapi.Models.ValueObject.BankFiles.Constants;
using nordelta.cobra.webapi.Models.ValueObject.BankFiles.GaliciaFiles;
using nordelta.cobra.webapi.Repositories.Contracts;
using nordelta.cobra.webapi.Services.Contracts;
using nordelta.cobra.webapi.Utils;
using Serilog;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using WinSCP;
using Monitoreo = Nordelta.Monitoreo;

namespace nordelta.cobra.webapi.Services
{
    public class GaliciaFilesService : IGaliciaFilesService
    {
        public readonly IArchivoDeudaRepository _archivoDeudaRepository;
        public readonly IPublishedDebtBankFileRepository _publishedDebtBankFileRepository;
        public readonly IProcessTransactionService _processTransactionService;
        private readonly ServiciosMonitoreadosConfiguration _servicesMonConfig;
        public readonly List<CertificateConfiguration> _galiciaCertificates;
        public readonly List<PublishDebtConfiguration> _galiciaBusinessUnits;
        public readonly WinScpConfiguration _galiciaSftp;

        public GaliciaFilesService(
            IArchivoDeudaRepository archivoDeudaRepository,
            IPublishedDebtBankFileRepository publishedDebtBankFileRepository,
            IOptionsMonitor<List<CertificateConfiguration>> certificatesConfig,
            IOptionsMonitor<List<PublishDebtConfiguration>> publishDebtConfig,
            IOptionsMonitor<WinScpConfiguration> winScpConfig,
            IProcessTransactionService processTransactionService,
            IOptions<ServiciosMonitoreadosConfiguration> servicesMonConfig
            )
        {
            _archivoDeudaRepository = archivoDeudaRepository;
            _publishedDebtBankFileRepository = publishedDebtBankFileRepository;
            _processTransactionService = processTransactionService;
            _servicesMonConfig = servicesMonConfig.Value;
            _galiciaCertificates = certificatesConfig.Get(CertificateConfiguration.GaliciaCertificates);
            _galiciaBusinessUnits = publishDebtConfig.Get(PublishDebtConfiguration.GaliciaBUConfig);
            _galiciaSftp = winScpConfig.Get(WinScpConfiguration.GaliciaSftpConfig);
        }

        public Session OpenSession()
        {
            var galiciaSftp = _galiciaSftp.SessionOptions;
            var session = new Session();

            var certificatePath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Certificates",
                galiciaSftp.SshPrivateKeyPath);
            if (!File.Exists(certificatePath))
            {
                throw new FileNotFoundException("Certificate doesn't exist.", certificatePath);
            }

            galiciaSftp.SshPrivateKeyPath = certificatePath;
            galiciaSftp.Timeout = TimeSpan.FromMinutes(5);
            try
            {
                galiciaSftp.AddRawSettings("ProxyPort", "0");
            }
            catch (Exception ex)
            {
                Log.Warning(ex, "Error in OpenSession when trying to add ProxyPort, the existing value will be used.");
            }

            session.Open(galiciaSftp);
            return session;
        }

        private static List<string> CreateDetails(List<DetalleDeuda> detallesDeuda)
        {
            var detailLines = new List<string>();
            var detailBuilder = new StringBuilder();

            foreach (var detalleDeuda in detallesDeuda)
            {
                try
                {
                    detailBuilder.Append("D"); //COD-REGISTRO
                    detailBuilder.Append("CUIT".PadLeft(4, ' ')); //TIP-IDENT- CLIENTE
                    detailBuilder.Append(detalleDeuda.NroCuitCliente.PadRight(15, ' ')); //IDENT - CLIENTE
                    detailBuilder.Append(detalleDeuda.NroCuitCliente.PadRight(18, ' ')); //IDENT-INTERNA-CLIENTE: CUIT - CUIL
                    detailBuilder.Append(detalleDeuda.TipoComprobante == "OP" ? "FC" : "PC"); //TIPO-DOCUMENTO
                    if (detalleDeuda.TipoComprobante == "OP") //FC
                    {
                        detailBuilder.Append(detalleDeuda.NroComprobante.PadRight(25, ' ')); //IDENT- DOCUMENTO
                    }
                    else
                    {
                        detailBuilder.Append("".PadLeft(25, ' ')); //IDENT- DOCUMENTO
                    }
                    detailBuilder.Append("".PadLeft(8, ' ')); //FECHA-VIGENCIA
                    if (detalleDeuda.TipoComprobante == "OP") //FC
                    {
                        detailBuilder.Append(detalleDeuda.ImportePrimerVenc.PadLeft(15, '0')); //IMPORTE-1
                        detailBuilder.Append(detalleDeuda.FechaPrimerVenc); //FECHA - VTO - 1
                        detailBuilder.Append(detalleDeuda.ImporteSegundoVenc.All(x => x == '0') ? "".PadLeft(15, ' ') : detalleDeuda.ImporteSegundoVenc.PadLeft(15, ' ')); //IMPORTE-2
                        detailBuilder.Append(detalleDeuda.FechaSegundoVenc.All(x => x == '0') ? "".PadLeft(8, ' ') : detalleDeuda.FechaSegundoVenc); //FECHA - VTO - 2
                    }
                    else
                    {
                        detailBuilder.Append("".PadLeft(15, ' ')); //IMPORTE-1
                        detailBuilder.Append("".PadLeft(8, ' ')); //FECHA - VTO - 1
                        detailBuilder.Append("".PadLeft(15, ' ')); //IMPORTE-2
                        detailBuilder.Append("".PadLeft(8, ' ')); //FECHA - VTO - 2
                    }
                    detailBuilder.Append("".PadLeft(15, ' ')); //IMPORTE-3
                    detailBuilder.Append("".PadLeft(8, ' ')); //FECHA - VTO - 3
                    detailBuilder.Append(detalleDeuda.NombreCliente.PadRight(30, ' ')); //NOMBRE-CLIENTE
                    detailBuilder.Append(detalleDeuda.Id.ToString().PadLeft(30, ' ')); //IDENT-INTERNA- DOCUMENTO : Confirmar que este campo se pueda usar tanto para FC como para PC
                    detailBuilder.Append("".PadLeft(6, ' ')); //DIVISION
                    detailBuilder.Append(detalleDeuda.CodigoMoneda.PadLeft(2, '0')); //COD-MONEDA
                    detailBuilder.Append("".PadLeft(38, ' ')); //LEYENDA-1
                    detailBuilder.Append("".PadLeft(52, ' ')); //FILLER*

                    detailLines.Add(detailBuilder.ToString());
                    detailBuilder.Clear();
                }
                catch (Exception ex)
                {
                    Log.Error(ex, "CreateDetails(): Ocurrió un error al intentar procesar detalle deuda con Id: {detalleDeudaId}", detalleDeuda.Id);
                    continue;
                }
            }

            Log.Debug("CreateDetails(): Se procesaron correctamente {numDetailLines} de {numDetailDebts} detalles deuda", detailLines.Count, detallesDeuda.Count);

            return detailLines;
        }

        public bool CreateDebtFilesForGalicia()
        {
            // Necessary folders for galicia files
            var folderPath = AppDomain.CurrentDomain.BaseDirectory;
            foreach (var nameFolder in new List<string> { "GaliciaFiles", "OUT" })
            {
                folderPath = Path.Combine(folderPath, nameFolder);
                if (!Directory.Exists(folderPath))
                    Directory.CreateDirectory(folderPath);
            }

            foreach (var file in new DirectoryInfo(Path.Combine(folderPath)).GetFiles())
                file.Delete();

            var builder = new StringBuilder();

            foreach (var BU in _galiciaBusinessUnits)
            {
                foreach (var acuerdo in BU.Acuerdos)
                {
                    try
                    {
                        Log.Information("CreateDebtFilesForGalicia(): Generando archivo deuda para Galicia usando BusinessUnit: {buCuit} y el acuerdo: {@acuerdo}", BU.Cuit, acuerdo);

                        var detalleDeudas = _archivoDeudaRepository.GetLastDetalleDeudasByCurrencyAndCuitEmpresa(acuerdo.Currency, BU.Cuit);

                        if (!detalleDeudas.Any())
                        {
                            Log.Information("CreateDebtFilesForGalicia(): No se encontrarón detalles deuda");
                            continue;
                        }

                        var archivoDeuda = detalleDeudas.GroupBy(x => x.ArchivoDeudaId, (x, y) => y.FirstOrDefault().ArchivoDeuda).FirstOrDefault();

                        var publishedDebt = _publishedDebtBankFileRepository.GetSingle(predicate: x => x.CodigoOrganismo == acuerdo.CodigoOrganismo
                                                                                                    && x.Bank == PaymentSource.Galicia,
                                                                                               orderBy: x => x.OrderByDescending(a => a.NroArchivo));

                        if (publishedDebt != null && publishedDebt.ArchivoDeudaId == archivoDeuda.Id)
                        {
                            Log.Information("CreateDebtFilesForGalicia(): El archivo deuda ya fue publicado a Galicia");
                            continue;
                        }

                        var nroArchivo = publishedDebt == null ? acuerdo.NroArchivo
                                       : publishedDebt.NroArchivo < acuerdo.NroArchivo ? acuerdo.NroArchivo
                                       : publishedDebt.NroArchivo + 1;

                        var dateNow = DateTime.Now;
                        var dateString = dateNow.ToString("yyyyMMdd");
                        var timeString = dateNow.ToString("hhmmss");
                        var lines = new List<string>();

                        // HEADER
                        builder.Append("H"); //COD - REGISTRO*
                        builder.Append(acuerdo.CodigoOrganismo.PadLeft(10, '0')); //IDENT-EMPRESA*
                        builder.Append(dateString); //FECHA-PROCESO
                        builder.Append(timeString); //HORA-PROCESO
                        builder.Append(nroArchivo.ToString().PadLeft(4, '0')); //NRO-ARCHIVO- ENV*
                        builder.Append("".PadLeft(271, ' ')); //FILLER

                        lines.Add(builder.ToString());
                        builder.Clear();

                        // DETAILS
                        var detailLines = CreateDetails(detalleDeudas);
                        lines.AddRange(detailLines);

                        // TRAILER
                        builder.Append("T"); //COD-REGISTRO
                        builder.Append((2 + detailLines.Count).ToString().PadLeft(9, '0')); //CANTIDAD-REG
                        builder.Append("".PadLeft(290, ' ')); //FILLER

                        lines.Add(builder.ToString());
                        builder.Clear();

                        // Create File
                        var fileName = $"FY{acuerdo.CodigoOrganismo}-{dateString}-{timeString}.txt";
                        var filePath = Path.Combine(folderPath, fileName);
                        File.AppendAllLines(filePath, lines.ToArray(), Encoding.ASCII);

                        var newSentFile = new PublishedDebtBankFile
                        {
                            ArchivoDeudaId = archivoDeuda.Id,
                            CuitEmpresa = BU.Cuit,
                            CodigoOrganismo = acuerdo.CodigoOrganismo,
                            NroArchivo = nroArchivo,
                            FileName = archivoDeuda.FileName,
                            Bank = PaymentSource.Galicia,
                            CreatedOn = LocalDateTime.GetDateTimeNow()
                        };

                        _publishedDebtBankFileRepository.Insert(newSentFile);
                        Log.Information("CreateDebtFilesForGalicia(): PublishedDebtBankFile creado: {@publishedDebtBankFile}", newSentFile);
                    }
                    catch (Exception ex)
                    {
                        Log.Error(ex, "CreateDebtFilesForGalicia(): Ocurrió un error al intentar generar archivo deuda para Galicia usando BusinessUnit: {cuit} y el acuerdo: {@acuerdo}", BU.Cuit, acuerdo);
                        continue;
                    }
                }
            }

            var files = new DirectoryInfo(folderPath).GetFiles("*.TXT").Select(x => new { x.Name, x.Length });

            if (files.Any())
            {
                Log.Information("CreateDebtFilesForGalicia(): Se generarón archivos deuda para Galicia - Files : {@files}", files);
                return true;
            }
            else
            {
                Log.Information("CreateDebtFilesForGalicia(): No se generarón archivos deuda para Galicia");
                return false;
            }
        }

        public async Task EncryptAllDebtFiles()
        {
            try
            {
                // Necessary folders for galicia files
                var folderPath = AppDomain.CurrentDomain.BaseDirectory;
                foreach (var nameFolder in new List<string> { "GaliciaFiles", "OUT", "Encrypted" })
                {
                    folderPath = Path.Combine(folderPath, nameFolder);
                    if (!Directory.Exists(folderPath))
                        Directory.CreateDirectory(folderPath);
                }

                foreach (var file in new DirectoryInfo(Path.Combine(folderPath)).GetFiles())
                    file.Delete();

                // Certificate folder
                var folderCertPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Certificates");
                if (!Directory.Exists(folderCertPath))
                    Directory.CreateDirectory(folderCertPath);

                var certificate = _galiciaCertificates.FirstOrDefault(x => x.Name == CertificateConfiguration.PublicKey);
                var fileCertPath = Path.Combine(folderCertPath, certificate.FileName);

                if (!File.Exists(fileCertPath))
                    throw new FileNotFoundException("Certificate doesn't exist.", fileCertPath);

                await PgpCoreManager.EncryptAllFiles(Directory.GetParent(folderPath).FullName, folderPath, fileCertPath);

                var files = new DirectoryInfo(folderPath).GetFiles("*.PGP").Select(x => new { x.Name, x.Length  });
                Log.Information("EncryptAllDebtFiles(): Se encriptarón archivos deuda para ser enviados al SFTP de Galicia - Files : {@files}", files);
            }
            catch (Exception ex)
            {
                Log.Error(ex, "EncryptAllDebtFiles(): Ocurrió un error al intentar encriptar archivos de deuda para ser enviados al SFTP de Galicia");
            }
        }

        public void UploadDebtFilesToGalicia()
        {
            try
            {
                // Necessary folders for galicia files
                var folderPath = AppDomain.CurrentDomain.BaseDirectory;
                foreach (var nameFolder in new List<string> { "GaliciaFiles", "OUT", "Encrypted" })
                {
                    folderPath = Path.Combine(folderPath, nameFolder);
                    if (!Directory.Exists(folderPath))
                        Directory.CreateDirectory(folderPath);
                }

                var remoteFolderPath = _galiciaSftp.OutputFolder;

                var files = new DirectoryInfo(folderPath).GetFiles("*.PGP").Select(x => new { x.Name , x.Length });

                if (files.Any())
                {
                    Log.Information("UploadDebtFilesToGalicia(): Se encontrarón archivos deuda para ser enviados al SFTP de Galicia - Files : {@files}", files);

                    var session = OpenSession();

                    TransferOptions transferOptions = new TransferOptions { TransferMode = TransferMode.Binary };
                    TransferOperationResult result = session.PutFiles(folderPath, remoteFolderPath, false, transferOptions);

                    result.Check();
                    session.Close();

                    Monitoreo.Monitor.Ok("Se publicaron correctamente las deudas", _servicesMonConfig.PubDeudaGalicia);
                }
                else
                {
                    Log.Warning("UploadDebtFilesToGalicia(): No se encontrarón archivos deuda para ser enviados al SFTP de Galicia");
                    Monitoreo.Monitor.Critical("Ocurrió un error al intentar publicar las deudas", _servicesMonConfig.PubDeudaGalicia);
                }
            }
            catch (Exception ex)
            {
                Log.Error(ex, "UploadDebtFilesToGalicia(): Ocurrió un error al intentar de subir archivos al SFTP de Galicia");
                Monitoreo.Monitor.Critical("Ocurrió un error al intentar publicar las deudas", _servicesMonConfig.PubDeudaGalicia);
                throw;
            }
        }

        public bool GetPaymentFilesOfGalicia()
        {
            try
            {
                // Necessary folders for galicia files
                var folderPath = AppDomain.CurrentDomain.BaseDirectory;
                foreach (var nameFolder in new List<string> { "GaliciaFiles", "IN" })
                {
                    folderPath = Path.Combine(folderPath, nameFolder);
                    if (!Directory.Exists(folderPath))
                        Directory.CreateDirectory(folderPath);
                }

                foreach (var file in new DirectoryInfo(folderPath).GetFiles())
                    file.Delete();

                var session = OpenSession();
                var remoteFolderPath = _galiciaSftp.InputFolder;

                TransferOptions transferOptions = new TransferOptions { TransferMode = TransferMode.Binary };

                var result = session.GetFiles(remoteFolderPath, folderPath, false, transferOptions);
                result.Check();
                session.Close();

                var files = new DirectoryInfo(folderPath).GetFiles("*.PGP").Select(x => new { x.Name, x.Length });

                var hasFiles = files.Any();
                if (hasFiles)
                {
                    Log.Information("GetPaymentFilesOfGalicia(): Se obtuvierón archivos de pagos del SFTP de Galicia - Files :  {@files}", files);
                }
                else
                {
                    Log.Information("GetPaymentFilesOfGalicia(): No se obtuvierón archivos de pagos del SFTP de Galicia");
                }
                return hasFiles;
            }
            catch (Exception ex)
            {
                Log.Error(ex, "GetPaymentFilesOfGalicia(): Ocurrió un error al intentar de obtener archivos de pago del SFTP de Galicia");
                throw;
            }
        }

        public async Task DecryptAllDebtFiles()
        {
            try
            {
                // Necessary folders for galicia files
                var folderPath = AppDomain.CurrentDomain.BaseDirectory;
                foreach (var nameFolder in new List<string> { "GaliciaFiles", "IN", "Decrypted" })
                {
                    folderPath = Path.Combine(folderPath, nameFolder);
                    if (!Directory.Exists(folderPath))
                        Directory.CreateDirectory(folderPath);
                }

                // Eliminar archivos viejos
                foreach (var file in new DirectoryInfo(Path.Combine(folderPath)).GetFiles())
                    file.Delete();

                // Certificate folder
                var folderCertPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Certificates");
                if (!Directory.Exists(folderCertPath))
                    Directory.CreateDirectory(folderCertPath);

                var certificate = _galiciaCertificates.FirstOrDefault(x => x.Name == CertificateConfiguration.PrivateKey);
                var privateKeyFilePath = Path.Combine(folderCertPath, certificate.FileName);

                if (!File.Exists(privateKeyFilePath))
                    throw new FileNotFoundException("Certificate doesn't exist.", privateKeyFilePath);

                await PgpCoreManager.DecryptAllFiles(Directory.GetParent(folderPath).FullName, folderPath, privateKeyFilePath, certificate.Password);

                var files = new DirectoryInfo(folderPath).GetFiles("*.TXT").Select(x => new { x.Name, x.Length });
                Log.Information("DecryptAllDebtFiles(): Se desencriptarón archivos de pagos obtenidos del SFTP de Galicia - Files : {@files}", files);
            }
            catch (Exception ex)
            {
                Log.Error(ex, "DecryptAllDebtFiles(): Ocurrió un error al intentar desencriptar archivos de pago obtenidos del SFTP de Galicia");
            }
        }

        private Dictionary<string, dynamic> GetPaymentRecordAndTheirDetails()
        {
            // Necessary folders for galicia files
            var folderPath = AppDomain.CurrentDomain.BaseDirectory;
            foreach (var nameFolder in new List<string> { "GaliciaFiles", "IN", "Decrypted" })
            {
                folderPath = Path.Combine(folderPath, nameFolder);
                if (!Directory.Exists(folderPath))
                    Directory.CreateDirectory(folderPath);
            }

            var ret = new Dictionary<string, dynamic>();
            var files = new DirectoryInfo(folderPath).GetFiles();
            var galiciaPayments = new List<PaymentGalicia>();

            foreach (var file in files)
            {
                try
                {
                    using TextReader tr = new StreamReader(file.Open(FileMode.Open));
                    {
                        var list = new List<PrGalicia>();
                        var engine = new MasterDetailEngine<HeaderGalicia, PrGalicia>(r =>
                        SelectorMasterOrDetail(r, GaliciaFilesConstants.Header));

                        var res = engine.ReadStream(tr);

                        foreach (var group in res)
                        {
                            if (_galiciaBusinessUnits.Exists(x => x.Acuerdos.Any(acuerdo => acuerdo.CodigoOrganismo == group.Master.IdEmpresa[6..])) && group.Details.Any())
                            {
                                list.AddRange(@group.Details);
                                foreach (var pago in list)
                                {
                                    galiciaPayments.Add(new PaymentGalicia { Header = group.Master, Pago = pago });
                                }
                            }
                        }

                        Log.Information("GetPaymentRecordAndTheirDetails(): El archivo {@file} fue procesado correctamente", new { file.Name, file.Length });

                        file.Delete();
                    }
                }
                catch (Exception ex)
                {
                    Log.Error(ex, "GetPaymentRecordAndTheirDetails(): Ocurrió un error al intentar procesar archivo de pago obtenido de Galicia: {@file}", new { file.Name, file.Length });
                }
            }

            ret.Add(GaliciaFilesConstants.PG, galiciaPayments);

            return ret;
        }

        private static RecordAction SelectorMasterOrDetail(string record, string masterToken)
        {
            if (record.Length < 2 || record.StartsWith('T'))
                return RecordAction.Skip;

            return record.StartsWith(masterToken) ? RecordAction.Master : RecordAction.Detail;
        }

        public async Task CreateAndPublishDebtFilesToGaliciaAsync()
        {
            // Si no se generan archivos deuda cancelo el proceso
            if(!CreateDebtFilesForGalicia())
                return;

            await EncryptAllDebtFiles();
            UploadDebtFilesToGalicia();
        }

        public async Task GetAndProcessPaymentFilesOfGaliciaAsync()
        {
            // Si no se obtienen archivos de pagos cancelo el proceso
            if (!GetPaymentFilesOfGalicia())
                return;

            await DecryptAllDebtFiles();
            var documentTypes = GetPaymentRecordAndTheirDetails();

            if (documentTypes.ContainsKey(GaliciaFilesConstants.PG))
            {
                _processTransactionService.ProcessRegistroFiles((List<PaymentGalicia>)documentTypes[GaliciaFilesConstants.PG]);
            }
        }
    }
}
