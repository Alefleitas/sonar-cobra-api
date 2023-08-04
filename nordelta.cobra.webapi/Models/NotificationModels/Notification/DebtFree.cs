using nordelta.cobra.webapi.Models.ArchivoDeuda;
using System.Collections.Generic;
using System;

namespace nordelta.cobra.webapi.Models
{
    public class DebtFree : NotificationType
    {
        public override Notification Evaluate(List<DetalleDeuda> detallesDeuda, List<Debin> debins, List<Communication> communications, List<SsoUser> users, DateTime date, out Dictionary<string, List<int>> dataMapper)
        {
            dataMapper = null;
            return null;
        }
    }
}
    