using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Options;
using nordelta.cobra.webapi.Configuration;
using nordelta.cobra.webapi.Controllers.ViewModels;
using nordelta.cobra.webapi.Repositories.Contracts;
using nordelta.cobra.webapi.Services;
using nordelta.cobra.webapi.Services.Contracts;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace nordelta.cobra.webapi.Models
{
    public class Email : DeliveryType
    {
        public override void Send(SsoUser recipient, string subject, string message) {
            this.Service.SendNotificationEmail(recipient.Email, subject, message);
        }
    }
}
