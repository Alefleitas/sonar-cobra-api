using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Threading.Tasks;
using nordelta.cobra.webapi.Models;
using nordelta.cobra.webapi.Models.ArchivoDeuda;
using nordelta.cobra.webapi.Services.DTOs;

namespace nordelta.cobra.webapi.Repositories.Contracts
{
    public interface IArchivoDeudaRepository
    {
        DetalleDeuda Find(int id);
        List<DetalleDeuda> FindMany(List<int> detalleDeudaIds);
        List<DetalleDeuda> All(string cuit = "", string accountNumber = "");
        List<DetalleDeuda> All(List<string> cuits);
        void Add(ArchivoDeuda archivoDeuda);
        void AddDetalle(List<DetalleDeuda> archivosDeuda);
        Task AddDetalleAsync(List<DetalleDeuda> detalleDeudas);
        ArchivoDeuda GetByFileName(string fileName);
        List<PropertyCode> GetPropertyCodes(List<string> cuits = null, string accountNumber = null);
        void Delete(ArchivoDeuda archivoDeuda);
        void Delete(List<ArchivoDeuda> archivosDeuda);
        DetalleDeuda FindSameDebtFromAnotherCurrency(DetalleDeuda debt);
        bool DetalleDeudaIsFromLastArchivoDeudaAvailable(int detalleDeudaId);
        List<PropertyCodeFull> GetPropertyCodesFull();
        List<DetalleDeuda> GetByFFileName(string Cuit, string FFileName);
        Task<ArchivoDeuda> GetByFileNameAsync(string fileName);
        List<DetalleDeuda> GetLastDetalleDeudas();
        List<DetalleDeuda> GetLastDetalleDeudasByCurrency(string currency);
        List<DetalleDeuda> GetDetalleDeudasByDebinIds(List<int> debinId);
        Task AddRepeatedDebtDetailsAsync(IEnumerable<RepeatedDebtDetail> debtDetails);
        Task<IEnumerable<RepeatedDebtDetail>> GetAllRepeatedDebtDetailsAsync();
        Task<int?> PostOrderAdvanceFees(List<AdvanceFee> advanceFees, User user);
        void CleanRepeatedDebtDetails();
        void SavePublicDebtRejectionFile(PublishDebtRejectionFile publishDebtRejectionFile);
        List<PublishDebtRejectionFile> GetPublishDebtRejectionFiles(DateTime? fechaDesde, DateTime? fechaHasta);
        Task SaveDebts(List<int> debtList, int debinId);
        Task<IEnumerable<AdvanceFee>> GetAdvancedFeesAsync(EAdvanceFeeStatus? status = null);
        AdvanceFee GetAdvancedFeesByOrderId(int orderId, Func<AdvanceFee, bool> predicate = null);
        Task SetAdvanceFeeOrdersStatus(List<int> ids, EAdvanceFeeStatus status);
        void UpdateInformedAdvanceFeeByIds(List<int> advanceFeeIds);
        Debin GetDebinByPaymentMethodId(int id);
        DetalleDeuda GetByObsLibrePraAndObsLibreSda(string obsLibrePrimera, string obsLibreSegunda);
        List<DetalleDeuda> GetLastDetalleDeudasByCurrencyAndCuitEmpresa(string currency, string cuitEmpresa);
        List<DetalleDeuda> GetRecentDebtsFromOldDebts(IEnumerable<DetalleDeuda> oldDebts);
        List<DetalleDeuda> GetDetallesDeudaByPaymentMethodId(int paymentMethodId);
        IEnumerable<DetalleDeuda> GetAllDetallesDeuda(Expression<Func<DetalleDeuda, bool>> predicate = null);
        List<int> GetLastArchivoDeudaIds(Expression<Func<ArchivoDeuda, bool>> predicate = null);
    }
}