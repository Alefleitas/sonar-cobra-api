using nordelta.cobra.webapi.Models;
using nordelta.cobra.webapi.Services.DTOs;
using System.Collections.Generic;

namespace nordelta.cobra.webapi.Services.Contracts
{
    public interface IPaymentMethodService
    {
        void InformAllPaymentMethodDone();
        CvuOperation CreateCvuOperation(OperationInformationResultDto operationInformationResultDto);
        List<PaymentMethodDto> GetPaymentMethods(PaymentInstrument? instrument = null);
        bool CreatePaymentMethod(PaymentMethodDto paymentMethodDto);
        PaymentMethod GetPaymentMethod(string operationId, PaymentSource source, PaymentInstrument instrument);
        List<User> GetAllUsersFromDebin();
        InformDebinPagination GetDebinesWithPagination(int pageSize, int pageNumber, string payerId, string fechaDesde, string fechaHasta);
        PaymentMethod Update(PaymentMethod paymentMethod);
        bool UpdatePaymentInformStatus(PaymentInformedDto paymentInformDto);
        void CheckAndFinalizePaymentInformed();
        
    }
}
