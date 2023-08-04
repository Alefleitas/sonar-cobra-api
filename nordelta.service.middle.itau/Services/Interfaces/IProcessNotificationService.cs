using Microsoft.AspNetCore.Mvc;
using nordelta.service.middle.itau.Models;
using nordelta.service.middle.itau.Services.DTOs;
using System.Net;

namespace nordelta.service.middle.itau.Services.Interfaces
{
    public interface IProcessNotificationService
    {
        Task<HttpResponseMessage?> ProcessNotificationAsync(string companySocialReason, TransactionResultDto transactionResultDto);
    }
}
