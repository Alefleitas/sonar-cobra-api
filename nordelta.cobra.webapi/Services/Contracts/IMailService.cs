using Microsoft.AspNetCore.Http;
using nordelta.cobra.webapi.Controllers.ViewModels;
using nordelta.cobra.webapi.Models;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace nordelta.cobra.webapi.Services.Contracts;

public interface IMailService
{
    bool sendContactEmail(ContactViewModel contact, IList<IFormFile> attachments);
    void SendNotificationEmail(string email, string subject, string message);
    void SendNotificationEmail(List<string> emails, string subject, string body);
    Task SendRepeatedDebtDetailsEmail(IEnumerable<RepeatedDebtDetail> details, IEnumerable<string> recipients);
    
    // Pasar como parámetro un modelo de LegalesDetail
    Task SendRepeatedLegalEmail(IEnumerable<DepartmentChangeNotification> details, IEnumerable<string> recipients);
    void SendEmailQuotationBot(string details, IEnumerable<string> recipients = null, Exception ex = null);
}
