using Newtonsoft.Json;
using nordelta.service.middle.itau.Services.DTOs;
using nordelta.service.middle.itau.Services.Interfaces;
using Serilog;
using System.Diagnostics;
using System.Net;
using System.Text;

namespace nordelta.service.middle.itau.Services
{
    public class ProcessNotificationService : IProcessNotificationService
    {
        private readonly HttpClient _httpClient;
        private readonly IConfiguration _configuration;
        public ProcessNotificationService(HttpClient httpClient,
                IConfiguration configuration
            )
        {
            _httpClient = httpClient;
            _configuration = configuration;
        }


        public async Task<HttpResponseMessage?> ProcessNotificationAsync(string companySocialReason, TransactionResultDto transactionResultDto)
        {
            try
            {
                Log.Debug("ProcessNotifications starting. Detail: \n {@companySocialReason}\n {@transactionResultDto}", transactionResultDto, companySocialReason);
               
                string cobraApiBaseUrl = _configuration.GetSection("CobraApi:Url").Value;
                string jsonData = JsonConvert.SerializeObject(transactionResultDto);
                string requestUrl = $"{cobraApiBaseUrl}/itau/ProcessNotification?companySocialReason={Uri.EscapeDataString(companySocialReason)}";


                HttpRequestMessage request = new HttpRequestMessage(HttpMethod.Post, requestUrl);
                request.Content = new StringContent(jsonData, Encoding.UTF8, "application/json");
                request.Headers.Add("Authorization", $"{_configuration.GetSection("CobraApi:Token").Value}");

                Log.Debug("Sending request. Detail: \n {@request}", request);

                HttpResponseMessage response = await _httpClient.SendAsync(request);
                response.EnsureSuccessStatusCode();

                Log.Debug("Getting response. Detail: \n {@response}", response);

                return response;
            }
            catch (Exception ex)
            {
                Log.Error("Error in ProcessNotificactions. Detail: \n {@ex}", ex);
                Debug.WriteLine(ex.Message);
                throw;
            }
        }


    }
}
