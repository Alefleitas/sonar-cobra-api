using nordelta.cobra.webapi.Services.Contracts;
using nordelta.cobra.webapi.Services.DTOs;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace nordelta.cobra.webapi.Models.NotificationModels.Delivery
{
    public class DeliveryTypeInjector : IDeliveryTypeInjector
    {
        private readonly IMailService _mailService;

        public DeliveryTypeInjector(IMailService mailService)
        {
            this._mailService = mailService;
        }

        public DeliveryType InjectService(DeliveryType deliveryType) {
            var type = deliveryType.GetType().Name;
            
            switch(type)
            {
                case "Email":
                    deliveryType.Service = this._mailService;
                    return deliveryType;
                case "Inbox":
                    return null;
                default: return null;
            }
        }
    }
}
