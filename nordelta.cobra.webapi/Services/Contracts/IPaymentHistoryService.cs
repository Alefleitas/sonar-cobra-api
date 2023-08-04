using System;
using System.Collections.Generic;
using nordelta.cobra.webapi.Services.DTOs;

namespace nordelta.cobra.webapi.Services.Contracts
{
    public interface IPaymentHistoryService
    {
        List<PaymentHistoryDto> GetAllApprovedDebin(string clientCuit);
    }
}
