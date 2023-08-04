using nordelta.cobra.webapi.Models.ArchivoDeuda;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;

namespace nordelta.cobra.webapi.Models
{
    public class PastDue : NotificationType
    {
        public override Notification Evaluate(List<DetalleDeuda> detallesDeuda, List<Debin> debins, List<Communication> communications, List<SsoUser> users, DateTime date, out Dictionary<string, List<int>> dataMapper)
        {
            dataMapper = new Dictionary<string, List<int>>();
            if (detallesDeuda == null)
                return null;

            var results = detallesDeuda.Where(x =>
                (DateTime.ParseExact(x.FechaPrimerVenc, "yyyyMMdd", CultureInfo.InvariantCulture).Date - date.Date).Days >= this.ConfigurationDays
                && x.PaymentMethod == null).GroupBy(y => y.NroCuitCliente);

            if (!results.Any())
                return null;

            List<User> recipients = new List<User>();
            foreach (var group in results)
            {
                var cuit = group.Key;
                var user = users.Where(x => x.Cuit == cuit).FirstOrDefault();
                if (user != null)
                {
                    var rolCheck = this.Roles.Any(r => user.Roles.Any(ur => ur.Role == r.Name));
                    if (rolCheck) {
                        dataMapper.Add(user.IdApplicationUser, group.Select(x => x.Id).ToList());
                        recipients.Add(new User() { Id = user.IdApplicationUser, Email = user.Email });
                    }
                }
            }

            return recipients.Count > 0 ?
                new Notification()
                {
                    Date = date,
                    NotificationType = this,
                    Recipients = recipients
                } : null;
        }
    }
}
