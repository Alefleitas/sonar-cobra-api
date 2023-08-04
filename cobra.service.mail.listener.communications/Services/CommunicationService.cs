using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http.Headers;
using System.Text;
using System.Threading.Tasks;
using cobra.service.mail.listener.communications.Configuration;
using cobra.service.mail.listener.communications.Models;
using Microsoft.Extensions.Options;
using ServiceStack;

namespace cobra.service.mail.listener.communications.Services
{
    public class CommunicationService : ICommunicationService
    {
        private readonly IOptions<CobraApiConfiguration> _cobraConfig;
        private readonly ILogger<CommunicationService> _communicationServiceLogger;

        public CommunicationService(ILogger<CommunicationService> communicationServiceLogger, IOptions<CobraApiConfiguration> cobraConfig)
        {
            _communicationServiceLogger = communicationServiceLogger;
            _cobraConfig = cobraConfig;
        }

        public async Task PostCommunication(ICollection<Communication> communications)
        {
            try
            {
                var communicationsRequest =
                    await $"{_cobraConfig.Value.Url}/api/v1/Communication/CreateCommunicationFromService"
                        .PostJsonToUrlAsync(communications,
                            req =>
                            {
                                req.Headers.Authorization = AuthenticationHeaderValue.Parse(_cobraConfig.Value.Token);
                            });
            }
            catch (Exception ex)
            {
                var isAnyClientError = ex.IsAny400();
                var isAnyServerError = ex.IsAny500();

                if (isAnyClientError || isAnyServerError)
                {
                    HttpStatusCode? errorStatus = ex.GetStatus();
                    string errorBody = ex.GetResponseBody();
                    _communicationServiceLogger.LogError(ex,
                        "Error sending request to {url}. ErrorStatus: {@status}. ErrorBody: {body}", _cobraConfig.Value.Url,
                        errorStatus, errorBody);
                }
                
                throw;
            }
           
        }
    }
}
