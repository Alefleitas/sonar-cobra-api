using nordelta.cobra.webapi.Controllers.ViewModels;
using nordelta.cobra.webapi.Services.DTOs;
using System.Collections.Generic;
using System;

namespace nordelta.cobra.webapi.Services.Contracts
{
    public interface IPaymentReportsService
    {
        PaymentReportCommandResponseDto CreatePaymentReport(PaymentReportDto report, string userId);
        IEnumerable<PaymentReportViewModel> GetPaymentResportsByDate(DateTime fromDate, DateTime toDate);
    }
}
