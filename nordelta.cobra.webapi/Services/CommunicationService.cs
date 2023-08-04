using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Mail;
using System.Threading.Tasks;
using nordelta.cobra.webapi.Controllers.ViewModels;
using nordelta.cobra.webapi.Models;
using nordelta.cobra.webapi.Repositories.Contracts;
using nordelta.cobra.webapi.Services.Contracts;
using nordelta.cobra.webapi.Utils;

namespace nordelta.cobra.webapi.Services
{
    public class CommunicationService: ICommunicationService
    {
        private readonly ICommunicationRepository _communicationRepository;
        private readonly IUserRepository _userRepository;
        private readonly IMailService _mailService;
        private readonly IAccountBalanceService _accountBalanceService;

        public CommunicationService(ICommunicationRepository communicationRepository, IUserRepository userRepository, IMailService mailService, IAccountBalanceService accountBalanceService)
        {
            this._communicationRepository = communicationRepository;
            this._userRepository = userRepository;
            this._mailService = mailService;
            this._accountBalanceService = accountBalanceService;
        }

        public Communication InsertOrUpdate(Communication communication, User user)
        {
            communication.CommunicationCreatorUserId = user.Id;
            communication.SsoUser = new SsoUser
            {
                IdApplicationUser = communication.CommunicationCreatorUserId,
                Email = user.Email
            };
            this._communicationRepository.InsertOrUpdate(communication);


            return communication;
        }

        public List<Communication> GetCommunicationsForAccountBalance(int accountBalanceId)
        {
            var communications =  this._communicationRepository.GetAllForAccountBalance(accountBalanceId);
            communications.ForEach(x => x.SsoUser = _userRepository.GetSsoUserById(x.CommunicationCreatorUserId));

            return communications;
        }

        public bool Delete(int id, User user)
        {
            this._communicationRepository.Delete(id,user);
            return true;
        }

        public bool TemplateToggle(int id)
        {
            return this._communicationRepository.ToggleTemplate(id);
        }

        public void SendNullProductNotification(string email, string subject, string body)
        {
            this._mailService.SendNotificationEmail(email, subject, body);
        }

        public void HandleCommunicationsFromService(List<CommunicationFromServiceViewModel> emails)
        {
            Serilog.Log.Information(@"HandleCommunicationsFromService: New Communication emails received, processing: {@emails}", emails);

            foreach (var email in emails)
            {
                var communications = new List<Communication>();

                try
                {
                    var accountBalances = _accountBalanceService.GetClientByProduct(email.Product);

                    if (accountBalances.Count > 0)
                    {
                        communications.AddRange(accountBalances.Select(accountBalance => new Communication
                            {
                                AccountBalanceId = accountBalance.Id,
                                CommunicationCreatorUserId = "System",
                                Date = LocalDateTime.GetDateTimeNow(),
                                Incoming = false,
                                Client = new User { Id = accountBalance.ClientId },
                                CommunicationChannel = EComChannelType.CorreoElectronico,
                                CommunicationResult = ECommunicationResult.Revisar,
                                Description = email.Body,
                            }));

                        _communicationRepository.Insert(communications);
                    }
                    else
                    {
                        Serilog.Log.Warning(@"HandleCommunicationsFromService: No account balances were found with product {prod}", email.Product);

                        string bodyContent = $"Este es un mail automático de Cobra.\n No se encontró el código de producto {email.Product} en el Sistema por lo que no se cargó ninguna comunicación.";
                        string subject = "Respuesta automática - Comunicaciones Cobra";
                        
                        SendNullProductNotification(email.Sender, subject, bodyContent);
                    }
                }
                catch (Exception ex)
                {
                    Serilog.Log.Error(ex, "Error in HandleCommunicationsFromService. Email: {@mail}", email);
                }
            }
        }
    }
}
