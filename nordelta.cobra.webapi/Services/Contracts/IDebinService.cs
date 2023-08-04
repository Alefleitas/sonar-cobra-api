using System;
using System.Collections.Generic;
using System.ServiceModel;
using System.Threading.Tasks;
using nordelta.cobra.webapi.Configuration;
using Hangfire.Server;
using nordelta.cobra.webapi.Controllers.ViewModels;
using nordelta.cobra.webapi.Models;
using nordelta.cobra.webapi.Services.DTOs;

namespace nordelta.cobra.webapi.Services.Contracts
{
    public interface IDebinService
    {
        /// <summary>
        /// It would create, publish and save a normal DEBIN to the specified account. Normal DEBIN require always acceptance from User on his Home Banking 
        /// </summary>
        /// <param name="publishDebinViewModel"></param>
        /// <param name="user"></param>
        /// <param name="debinExpirationTimeInMinutes"> If 0, would take default value from AppSettings</param>
        /// <returns></returns>
        Task PublishDebin(PublishDebinViewModel publishDebinViewModel, User user, int debinExpirationTimeInMinutes = 0);
        Task<PaymentStatus> GetDebinStatus(string debinCode); //consulta BD
        Task<PaymentStatus> GetDebinState(GetDebinStateRequest debinStateRequest); //consulta ITAU
        Task<string> PublishExternDebin(ExternDebinViewModel debinData);
        void CheckEveryDebinStateAndSendRequestOnStatusChanged(PerformContext context);
        void InformPaymentDebinDoneManual(List<ManuallyInformPaymentDto> manuallyInformPayments);
    }
}