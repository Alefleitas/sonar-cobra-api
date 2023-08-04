using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System;
using nordelta.cobra.webapi.Controllers.ActionFilters;
using nordelta.cobra.webapi.Models;
using nordelta.cobra.webapi.Services.Contracts;
using nordelta.cobra.webapi.Services.DTOs;
using Serilog;
using Hangfire;
using Newtonsoft.Json;

namespace nordelta.cobra.webapi.Controllers
{
    public class ItauController : BaseApiController
    {
        private readonly IProcessTransactionService _processTransactionService;
        private readonly IBackgroundJobClient _backgroundJobClient;

        public ItauController(IProcessTransactionService processTransactionService, IBackgroundJobClient backgroundJobClient)
        {
            _processTransactionService = processTransactionService;
            _backgroundJobClient = backgroundJobClient;
        }

        [AuthToken(new EPermission[] { EPermission.Access_Payments })]
        [HttpPost]
        public ActionResult ProcessNotification([FromQuery] string companySocialReason, [FromBody] TransactionResultDto transactionResult)
        {
            try
            {
                Log.Information($"Received Notification from Middle Itau Api for company {companySocialReason} : {JsonConvert.SerializeObject(transactionResult)} ");
                _backgroundJobClient.Schedule(() => _processTransactionService.ProcessTransactionResult(transactionResult, companySocialReason), TimeSpan.FromSeconds(5));
                return new OkResult();

            }
            catch (Exception ex)
            {
                throw new Exception("Error: al intentar ejecutar ProcessTransactionResult");
            }
        }
    }
}
