using nordelta.cobra.webapi.Models;
using nordelta.cobra.webapi.Models.NotificationModels.Delivery;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace nordelta.cobra.webapi.Services.Contracts
{
    public interface IDeliveryTypeInjector
    {
        DeliveryType InjectService(DeliveryType deliveryType);
    }
}
