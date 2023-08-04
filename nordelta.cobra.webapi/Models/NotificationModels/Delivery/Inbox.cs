using nordelta.cobra.webapi.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace nordelta.cobra.webapi.Models
{
    public class Inbox : DeliveryType
    {
        public override void Send(SsoUser user, string subject, string Message)
        {
            throw new NotImplementedException();
        }
    }
}
