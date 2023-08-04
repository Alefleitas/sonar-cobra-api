using nordelta.cobra.webapi.Models;
using nordelta.cobra.webapi.Services.DTOs;
using System.Collections.Generic;

namespace nordelta.cobra.webapi.Services.Contracts
{
    public interface IPaymentDetailService
    {
        bool UpdateAll(List<PaymentDetail> paymentDetails);
        bool CreateAll(List<PaymentDetail> paymentDetails);
        List<PaymentDetailDto> GetAllByPaymentMethodId(int PaymentMethodId);
        bool HasPaymentDetail(int paymentMethodId);
    }
}
