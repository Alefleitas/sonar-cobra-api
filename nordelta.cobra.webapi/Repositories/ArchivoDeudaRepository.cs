using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using nordelta.cobra.webapi.Models;
using nordelta.cobra.webapi.Models.ArchivoDeuda;
using nordelta.cobra.webapi.Repositories.Contexts;
using nordelta.cobra.webapi.Repositories.Contracts;
using nordelta.cobra.webapi.Utils;
using Serilog;

namespace nordelta.cobra.webapi.Repositories
{
    public class ArchivoDeudaRepository : IArchivoDeudaRepository
    {
        private readonly RelationalDbContext _context;
        private readonly IUserChangesLogRepository _userChangesLogRepository;

        public ArchivoDeudaRepository(RelationalDbContext context, IUserChangesLogRepository userChangesLogRepository)
        {
            _context = context;
            _userChangesLogRepository = userChangesLogRepository;
        }

        public List<DetalleDeuda> All(string cuit = "", string accountNumber = "")
        {
            var detallesQueryable = GetLastDetallesDeudaQueryable();

            if (!string.IsNullOrEmpty(cuit))
            {
                detallesQueryable = detallesQueryable.Where(d => d.NroCuitCliente == cuit);
            }
            if (!string.IsNullOrEmpty(accountNumber))
            {
                detallesQueryable = detallesQueryable.Where(x => x.DescripcionLocalidad == accountNumber);
            }

            List<DetalleDeuda> result = PopulatePaymentMethodDebts(detallesQueryable);

            return result;
        }



        public List<DetalleDeuda> All(List<string> cuits)
        {
            var detallesQueryable = GetLastDetallesDeudaQueryable();

            if (cuits.Any())
            {
                detallesQueryable = detallesQueryable.Where(d => cuits.Contains(d.NroCuitCliente));
            }

            List<DetalleDeuda> result = PopulatePaymentMethodDebts(detallesQueryable);

            return result;
        }

        private IQueryable<DetalleDeuda> GetLastDetallesDeudaQueryable()
        {
            IQueryable<DetalleDeuda> detallesQueryable = _context.DetallesDeuda
                .Include(x => x.PaymentMethod)
                //.ThenInclude(x => x.Debts)
                .Include(x => x.PaymentReport)
                .Include(x => x.ArchivoDeuda)
                .Include(x => x.ArchivoDeuda.Trailer)
                .Include(x => x.ArchivoDeuda.Header)
                .Include(x => x.ArchivoDeuda.Header.Organismo);

            // TODO: Que filtre por el tiempo maximo de valides del archivo, cosa de que si no mandan por muchos
            // dias un archivo, no se muestre nada

            List<int> lastArchivosIds =
                _context.ArchivosDeuda.GroupBy(x => x.FormatedFileName, (x, y) => y.Max(z => z.Id)).ToList();

            detallesQueryable = detallesQueryable.Where(d => lastArchivosIds.Contains(d.ArchivoDeudaId)).OrderBy(x => x.FechaPrimerVenc); ;
            return detallesQueryable;
        }
        private List<DetalleDeuda> PopulatePaymentMethodDebts(IQueryable<DetalleDeuda> detallesQueryable)
        {
            var orderedDetails = detallesQueryable.OrderBy(x => x.FechaPrimerVenc).ToList();
            foreach (var detalleDeuda in orderedDetails.Where(x => x.PaymentMethod != null))
            {
                detalleDeuda.PaymentMethod.Debts = _context.DetallesDeuda
                    .Include(x => x.PaymentMethod)
                    .Where(x => x.PaymentMethodId == detalleDeuda.PaymentMethodId).ToList();
            }
            return orderedDetails;
        }

        public List<DetalleDeuda> GetByFFileName(string Cuit, string FFileName)
        {
            IQueryable<DetalleDeuda> detallesQueryable = _context.DetallesDeuda.AsQueryable();

            List<int> archivosIds = _context.ArchivosDeuda
                .Where(x => x.FormatedFileName == FFileName)
                .Select(y => y.Id)
                .ToList();

            detallesQueryable = detallesQueryable.Where(d => archivosIds.Contains(d.ArchivoDeudaId) && d.NroCuitCliente == Cuit).OrderBy(x => x.FechaPrimerVenc);

            List<DetalleDeuda> detalles = detallesQueryable
                .Include(x => x.PaymentMethod)
                .Include(x => x.ArchivoDeuda)
                .Include(x => x.ArchivoDeuda.Trailer)
                .Include(x => x.ArchivoDeuda.Header)
                .Include(x => x.ArchivoDeuda.Header.Organismo).ToList();

            return detalles;
        }

        public void Add(ArchivoDeuda archivoDeuda)
        {
            _context.ArchivosDeuda.Add(archivoDeuda);
            _context.SaveChanges();
        }
        public void AddDetalle(List<DetalleDeuda> detalleDeudas)
        {
            try
            {
                _context.DetallesDeuda.AddRange(detalleDeudas);
                _context.SaveChanges();
            }
            catch (Exception e)
            {
                Log.Error(e, "Failed to load DetalleDeuda {@detalle}", detalleDeudas);
            }
        }
        public async Task AddDetalleAsync(List<DetalleDeuda> detalleDeudas)
        {
            try
            {
                await _context.DetallesDeuda.AddRangeAsync(detalleDeudas);
                await _context.SaveChangesAsync();
            }
            catch (Exception e)
            {
                Log.Error(e, "Failed to load DetalleDeuda {@detalle}", detalleDeudas);
            }
        }
        public ArchivoDeuda GetByFileName(string fileName)
        {
            return _context.ArchivosDeuda.FirstOrDefault(e => e.FileName.Equals(fileName));
        }
        public async Task<ArchivoDeuda> GetByFileNameAsync(string fileName)
        {
            return await _context.ArchivosDeuda.FirstOrDefaultAsync(e => e.FileName.Equals(fileName));
        }
        public bool DetalleDeudaIsFromLastArchivoDeudaAvailable(int detalleDeudaId)
        {
            ArchivoDeuda archivoDeuda = Find(detalleDeudaId)?.ArchivoDeuda;
            if (archivoDeuda != null)
            {
                int lastArchivoDeudaId = _context.ArchivosDeuda
                    .Where(x => x.FormatedFileName == archivoDeuda.FormatedFileName)
                    .Max(x => x.Id);

                return archivoDeuda.Id == lastArchivoDeudaId;
            }
            return false;
        }

        public List<PropertyCodeFull> GetPropertyCodesFull()
        {
            IQueryable<DetalleDeuda> detallesQueryable = _context.DetallesDeuda.AsQueryable();

            List<int> archivosIds = _context.ArchivosDeuda
                .Select(x => x.Id)
                .ToList();

            detallesQueryable = detallesQueryable.Where(d => archivosIds.Contains(d.ArchivoDeudaId));

            List<PropertyCodeFull> codes = detallesQueryable
                .GroupBy(e => new
                {
                    e.NroComprobante,
                    e.ArchivoDeuda.Header.Organismo.CuitEmpresa,
                    e.NroCuitCliente,
                    e.CodigoMoneda,
                    e.ArchivoDeuda.FormatedFileName,
                    e.ArchivoDeuda.TimeStamp,
                    e.ObsLibreCuarta
                })
                .Select(g => new PropertyCodeFull()
                {
                    NroComprobante = g.Key.NroComprobante,
                    CuitEmpresa = g.Key.CuitEmpresa,
                    NroCuitCliente = g.Key.NroCuitCliente,
                    FormatedFileName = g.Key.FormatedFileName,
                    TimeStamp = g.Key.TimeStamp,
                    ProductCode = g.Key.ObsLibreCuarta.Trim()
                })
                .OrderBy(e => e.NroComprobante)
                .ToList();

            return codes;
        }

        public List<PropertyCode> GetPropertyCodes(List<string> cuits = null, string accountNumber = null)
        {
            IQueryable<DetalleDeuda> detallesQueryable = _context.DetallesDeuda
                .Include(x => x.ArchivoDeuda)
                .ThenInclude(x => x.Header)
                .ThenInclude(x => x.Organismo)
                .AsQueryable();

            List<int> lastArchivosIds = _context.ArchivosDeuda.GroupBy(x => x.FormatedFileName, (x, y) => y.Max(z => z.Id)).ToList();

            detallesQueryable = detallesQueryable.Where(d => lastArchivosIds.Contains(d.ArchivoDeudaId));

            if (cuits != null && cuits.Any())
            {
                detallesQueryable = detallesQueryable.Where(x => cuits.Contains(x.NroCuitCliente));
            }
            if (!string.IsNullOrEmpty(accountNumber))
            {
                detallesQueryable = detallesQueryable.Where(x => x.DescripcionLocalidad == accountNumber);
            }

            List<PropertyCode> codes = GetPropertyCodesFromQueryable(detallesQueryable, !string.IsNullOrEmpty(accountNumber));

            return codes;
        }

        public void Delete(ArchivoDeuda archivoDeuda)
        {
            _context.ArchivosDeuda.Remove(archivoDeuda);
            _context.SaveChanges();
        }

        public void Delete(List<ArchivoDeuda> archivosDeuda)
        {
            _context.ArchivosDeuda.RemoveRange(archivosDeuda);
            _context.SaveChanges();
        }

        public DetalleDeuda Find(int detalleDeudaId)
        {
            return _context.DetallesDeuda.Where(x => x.Id == detalleDeudaId)
                  .Include(x => x.PaymentMethod)
                  .Include(x => x.ArchivoDeuda)
                  .Include(x => x.ArchivoDeuda.Trailer)
                  .Include(x => x.ArchivoDeuda.Header)
                  .Include(x => x.ArchivoDeuda.Header.Organismo).Single();
        }

        public List<DetalleDeuda> FindMany(List<int> detalleDeudaIds)
        {
            return _context.DetallesDeuda.Where(x => detalleDeudaIds.Contains(x.Id))
                .Include(x => x.ArchivoDeuda)
                .Include(x => x.ArchivoDeuda.Trailer)
                .Include(x => x.ArchivoDeuda.Header)
                .Include(x => x.ArchivoDeuda.Header.Organismo)
                .ToList();
        }

        public DetalleDeuda FindSameDebtFromAnotherCurrency(DetalleDeuda debt)
        {
            List<DetalleDeuda> allForThisUser = All(debt.NroCuitCliente);

            // TODO: When more than two currencies will be working, this is gonna change to return a list
            DetalleDeuda aux = allForThisUser.Find(x =>
             x.NroComprobante == debt.NroComprobante
            && x.FechaPrimerVenc == debt.FechaPrimerVenc
            && x.ObsLibreSegunda == debt.ObsLibreSegunda // like: X-9999-00000075
            && x.NroCuitCliente == debt.NroCuitCliente
            && x.CodigoMoneda != debt.CodigoMoneda);

            return aux;
        }

        public List<DetalleDeuda> GetLastDetalleDeudas()
        {
            IQueryable<DetalleDeuda> detallesQueryable = _context.DetallesDeuda.AsQueryable();

            List<int> lastArchivosIds = _context.ArchivosDeuda.GroupBy(x => x.FormatedFileName, (x, y) => y.Max(z => z.Id)).ToList();

            return detallesQueryable.Where(d => lastArchivosIds.Contains(d.ArchivoDeudaId)).ToList();
        }

        public List<DetalleDeuda> GetLastDetalleDeudasByCurrency(string currency)
        {
            IQueryable<DetalleDeuda> detallesQueryable = _context.DetallesDeuda.AsQueryable();

            List<int> lastArchivosIds = _context.ArchivosDeuda.GroupBy(x => x.FormatedFileName, (x, y) => y.Max(z => z.Id)).ToList();

            return detallesQueryable.Where(d => lastArchivosIds.Contains(d.ArchivoDeudaId) && d.CodigoMoneda == currency)
                                    .Include(x => x.ArchivoDeuda)
                                    .Include(x => x.ArchivoDeuda.Trailer)
                                    .Include(x => x.ArchivoDeuda.Header)
                                    .Include(x => x.ArchivoDeuda.Header.Organismo).ToList();
        }

        public List<DetalleDeuda> GetDetalleDeudasByDebinIds(List<int> debinId)
        {
            return _context.DetallesDeuda.Where(x => debinId.Any(z => z == x.PaymentMethod.Id))
                  .Include(x => x.ArchivoDeuda)
                  .Include(x => x.ArchivoDeuda.Trailer)
                  .Include(x => x.ArchivoDeuda.Header)
                  .Include(x => x.ArchivoDeuda.Header.Organismo).ToList();
        }

        public List<PublishDebtRejectionFile> GetPublishDebtRejectionFiles(DateTime? fechaDesde, DateTime? fechaHasta)
        {
            IQueryable<PublishDebtRejectionFile> archivoDeudaRechazo = _context.ArchivoDeudaRechazo.AsQueryable();

            if (fechaDesde.HasValue && fechaHasta.HasValue)
            {
                archivoDeudaRechazo = archivoDeudaRechazo.Where(x => x.FileDate >= fechaDesde.Value && x.FileDate <= fechaHasta.Value);
            }
            else if (fechaDesde.HasValue)
            {
                archivoDeudaRechazo = archivoDeudaRechazo.Where(x => x.FileDate >= fechaDesde.Value);
            }
            else if (fechaHasta.HasValue)
            {
                archivoDeudaRechazo = archivoDeudaRechazo.Where(x => x.FileDate <= fechaHasta.Value);
            }

            return archivoDeudaRechazo
                .Include(x => x.PublishDebtRejections)
                .ThenInclude(y => y.Errors)
                .ToList();
        }

        public async Task AddRepeatedDebtDetailsAsync(IEnumerable<RepeatedDebtDetail> debtDetails)
        {
            await _context.RepeatedDebtDetail.AddRangeAsync(debtDetails);
            await _context.SaveChangesAsync();
        }
        public async Task<IEnumerable<RepeatedDebtDetail>> GetAllRepeatedDebtDetailsAsync()
        {
            return await _context.RepeatedDebtDetail.ToListAsync();
        }
        public void CleanRepeatedDebtDetails()
        {
            _context.Database.ExecuteSqlRaw("TRUNCATE TABLE [RepeatedDebtDetails]");
        }

        public void SavePublicDebtRejectionFile(PublishDebtRejectionFile publishDebtRejectionFile)
        {
            _context.ArchivoDeudaRechazo.Add(publishDebtRejectionFile);
            _context.SaveChanges();
        }

        public async Task SaveDebts(List<int> debtList, int debinId)
        {
            var debts = _context.DetallesDeuda.Where(x => debtList.Contains(x.Id)).ToList();
            var debin = _context.Debin.Find(debinId);
            foreach (var debt in debts)
            {
                debt.PaymentMethod = debin;
            }
            await _context.SaveChangesAsync();
        }

        public async Task<int?> PostOrderAdvanceFees(List<AdvanceFee> advanceFees, User user)
        {
            int? nuevoId = null;

            var executionStrategy = _context.Database.CreateExecutionStrategy();
            await executionStrategy.Execute(async () =>
            {
                using (var transaction = await _userChangesLogRepository.UnitOfWork.BeginTransactionAsync())
                {
                    if(advanceFees.Count == 1)
                    {
                        advanceFees[0].AutoApproved = true;
                    }
                    _context.AdvanceFees.AddRange(advanceFees);
                    await _context.SaveChangesAsync();

                    nuevoId = advanceFees.Count == 1 ? advanceFees[0].Id : (int?)null;

                    AddLog(advanceFees, user);

                    await _userChangesLogRepository.UnitOfWork.CommitAsync(transaction);

                }
            });

            return nuevoId;
        }


        public async Task<IEnumerable<AdvanceFee>> GetAdvancedFeesAsync(EAdvanceFeeStatus? status = null)
        {
            if (status.HasValue)
            {
                return (status == EAdvanceFeeStatus.Aprobado)
                    ? _context.AdvanceFees.Where(x => x.Status == status && x.Informed == false)
                    : _context.AdvanceFees.Where(x => x.Status == status);
            }
            return _context.AdvanceFees;
        }


        public AdvanceFee GetAdvancedFeesByOrderId(int orderId, Func<AdvanceFee, bool> predicate = null)
        {
            if (orderId > 0)
            {
                predicate = predicate ?? (x => true);

                return _context.AdvanceFees.FirstOrDefault(x => x.Id == orderId && predicate(x));
            }

            return new AdvanceFee();
        }

        public void UpdateInformedAdvanceFeeByIds(List<int> advanceFeeIds)
        {
            var advanceFees = _context.AdvanceFees.Where(x => advanceFeeIds.Contains(x.Id)).ToList();
            foreach (var advanceFee in advanceFees)
                advanceFee.Informed = true;
            _context.SaveChanges();
        }

        public async Task SetAdvanceFeeOrdersStatus(List<int> ids, EAdvanceFeeStatus status)
        {
            var advanceFees = _context.AdvanceFees.Where(x => ids.Contains(x.Id)).ToList();
            foreach (var advanceFee in advanceFees)
                advanceFee.Status = status;
            await _context.SaveChangesAsync();
        }

        public Debin GetDebinByPaymentMethodId(int id)
        {
            return _context.Debin.FirstOrDefault(x => x.Id == id);
        }

        private void AddLog(List<AdvanceFee> advanceFees, User user)
        {
            List<UserChangesLog> userChangesLogs = new List<UserChangesLog>();
            foreach (var df in advanceFees)
            {
                userChangesLogs.Add(new UserChangesLog
                {
                    EntityId = df.Id,
                    ModifiedEntity = CobraEntity.AdvanceFee,
                    ModifiedField = nameof(df.Id),
                    UserEmail = user.Email,
                    UserId = user.Id,
                    ModifyDate = LocalDateTime.GetDateTimeNow()
                });
            }
            _userChangesLogRepository.AddRange(userChangesLogs);
        }

        public DetalleDeuda GetByObsLibrePraAndObsLibreSda(string obsLibrePrimera, string obsLibreSegunda)
        {
            return _context.DetallesDeuda.FirstOrDefault(x => x.ObsLibrePrimera.Trim() == obsLibrePrimera && x.ObsLibreSegunda.Trim() == obsLibreSegunda);
        }

        public List<DetalleDeuda> GetRecentDebtsFromOldDebts(IEnumerable<DetalleDeuda> oldDebts)
        {
            var lastArchivosIds = _context.ArchivosDeuda.GroupBy(x => x.FormatedFileName, (x, y) => y.Max(z => z.Id)).ToList();
            var lastDebts = _context.DetallesDeuda.Where(d => lastArchivosIds.Contains(d.ArchivoDeudaId)).ToList();
            var ret = lastDebts.Where(d => oldDebts.Any(dd => dd.ObsLibreCuarta.Equals(d.ObsLibreCuarta) &&
                                                              dd.NroCuitCliente.Equals(d.NroCuitCliente) &&
                                                              dd.CodigoMoneda.Equals(d.CodigoMoneda) &&
                                                              dd.FechaPrimerVenc.Equals(d.FechaPrimerVenc))).ToList();

            return ret;
        }

        public List<DetalleDeuda> GetDetallesDeudaByPaymentMethodId(int paymentMethodId)
        {
            return _context.DetallesDeuda.Where(d => d.PaymentMethodId == paymentMethodId)
                                         .Include(x => x.ArchivoDeuda)
                                         .Include(x => x.ArchivoDeuda.Trailer)
                                         .Include(x => x.ArchivoDeuda.Header)
                                         .Include(x => x.ArchivoDeuda.Header.Organismo).ToList();
        }

        public List<DetalleDeuda> GetLastDetalleDeudasByCurrencyAndCuitEmpresa(string currency, string cuitEmpresa)
        {
            var detalleDeudas = new List<DetalleDeuda>();

            detalleDeudas.AddRange(GetLastDetalleDeudasByCurrency(currency)
                             .Where(d => d.ArchivoDeuda.Header.Organismo.CuitEmpresa == cuitEmpresa));

            return detalleDeudas;
        }

        public IEnumerable<DetalleDeuda> GetAllDetallesDeuda(Expression<Func<DetalleDeuda, bool>> predicate = null)
        {
            IQueryable<DetalleDeuda> query = _context.DetallesDeuda;

            if (predicate != null)
                query = query.Where(predicate);

            return query;
        }

        public List<int> GetLastArchivoDeudaIds(Expression<Func<ArchivoDeuda, bool>> predicate = null)
        {
            if (predicate != null)
                return _context.ArchivosDeuda.Where(predicate).GroupBy(x => x.FormatedFileName, (x, y) => y.Max(z => z.Id)).ToList();

            return _context.ArchivosDeuda.GroupBy(x => x.FormatedFileName, (x, y) => y.Max(z => z.Id)).ToList();
        }

        private List<PropertyCode> GetPropertyCodesFromQueryable(IQueryable<DetalleDeuda> detallesQueryable, bool hasAccountNumber)
        {
            var groupByKeys = hasAccountNumber
                ? detallesQueryable.GroupBy(e => new { e.NroComprobante, e.ArchivoDeuda.Header.Organismo.CuitEmpresa, e.NroCuitCliente, e.DescripcionLocalidad, e.ObsLibreCuarta })
                : detallesQueryable.GroupBy(e => new { e.NroComprobante, e.ArchivoDeuda.Header.Organismo.CuitEmpresa, e.NroCuitCliente, DescripcionLocalidad = (string)null, e.ObsLibreCuarta });

            return groupByKeys
                .Select(g => new PropertyCode()
                {
                    NroComprobante = g.Key.NroComprobante,
                    CuitEmpresa = g.Key.CuitEmpresa,
                    NroCuitCliente = g.Key.NroCuitCliente,
                    AccountNumber = hasAccountNumber ? g.Key.DescripcionLocalidad.Trim() : null,
                    ProductCode = g.Key.ObsLibreCuarta.Trim()
                })
                .OrderBy(e => e.NroComprobante)
                .ToList();
        }
    }
}
