using System;
using System.Collections.Generic;
using System.Linq;
using System.ServiceModel;
using System.Threading.Tasks;
using DebitoInmediatoServiceItau;
using Microsoft.Extensions.Options;
using Newtonsoft.Json;
using nordelta.cobra.webapi.Configuration;
using nordelta.cobra.webapi.Controllers.ViewModels;
using nordelta.cobra.webapi.Models;
using nordelta.cobra.webapi.Repositories.Contracts;
using nordelta.cobra.webapi.Services.Contracts;
using nordelta.cobra.webapi.Services.DTOs;

namespace nordelta.cobra.webapi.Services
{
    public class AnonymousPaymentsService : IAnonymousPaymentsService
    {
        IDebinService _debinService;
        IAnonymousPaymentRepository _anonymousPaymentRepository;
        ICompanyRepository _companyRepository;


        public AnonymousPaymentsService(
            IDebinService debinService,
            IAnonymousPaymentRepository anonymousPaymentRepository,
            ICompanyRepository companyRepository,
            IOptionsMonitor<ItauWCFConfiguration> itauConfiguration
        )
        {
            this._debinService = debinService;
            this._anonymousPaymentRepository = anonymousPaymentRepository;
            this._companyRepository = companyRepository;
        }

        //TODO next stage
        public void checkMigrationsForAnonymousPayments(DateTime date)
        {
            //Gets only anonymousPayments that where not migrated yet
            var pendingAPs = _anonymousPaymentRepository.GetPendingForMigration();

            foreach (var pendingAP in pendingAPs)
            {
                //checks if exists any user for the payment

            }
        }

        public async Task<string> PublishExternDebin(ExternDebinViewModel debinData)
        {
            try {
                return await _debinService.PublishExternDebin(debinData);
            } catch(Exception e) {
                throw new Exception(e.Message);
            }
        }

        public async Task<DebinStatusViewModel> GetDebinStatus(string debinCode)
        {
            var anonymousPayment = await _anonymousPaymentRepository.GetByDebinCode(debinCode);
            if (anonymousPayment != null)
            {
                return new DebinStatusViewModel
                {
                    Codigo = anonymousPayment.Status,
                    Descripcion = anonymousPayment.Status.ToString()
                };
            } 
            else 
            {
                throw new Exception("No se encontró el debin solicitado");
            }
        }
    }
}
