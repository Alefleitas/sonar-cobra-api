using nordelta.cobra.webapi.Services.DTOs;
using System.Collections.Generic;
using System.Threading.Tasks;
using nordelta.cobra.webapi.Controllers.ViewModels;
using nordelta.cobra.webapi.Models.ArchivoDeuda;
using nordelta.cobra.webapi.Models;

namespace nordelta.cobra.webapi.Services.Contracts
{
    public interface IPaymentsFilesService
    {
        Task<List<DetalleDeuda>> ProcessPaymentFileAsync(string fileName);
        Task ProcessSingleRejectionFileAsync(string filePath, PublishDebtRejectionFile publishDebtRejectionFile);
        Task ProcessSingleFileAsync(string file);
        Task SendRepeatedDebtDetailsEmail();
        void ProcessAllFiles();
        void ProcessAllRejectionFiles();
        List<PublishDebtRejectionFile> GetPublishDebtRejections(FilterReportByDatesViewModelRequest dates);
        Task<List<RepeatedDebsDetailsViewModel>> GetAllRepeatedDebtDetails();
        Task<int?> PostOrderAdvanceFee(List<OrderAdvanceFeeViewModel> advanceFees, User user);

        Task<IEnumerable<AdvanceFeeDto>> GetAdvanceFeeOrdersAsync();
        Task ChangeAdvanceFeeOrdersStatusAsync(List<int> ids, EAdvanceFeeStatus status);
        IEnumerable<PublishedDebtFile> GetAllPublishedDebtFiles();
        void NotifyOneApprovedAdvanceFee(int orderId);
    }
}