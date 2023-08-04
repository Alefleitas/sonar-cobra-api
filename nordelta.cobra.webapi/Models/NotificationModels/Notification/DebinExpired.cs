using nordelta.cobra.webapi.Models.ArchivoDeuda;
using System;
using System.Collections.Generic;
using System.Linq;

namespace nordelta.cobra.webapi.Models
{
    public class DebinExpired : NotificationType
    {
        public override Notification Evaluate(List<DetalleDeuda> detallesDeuda, List<Debin> debins, List<Communication> communications, List<SsoUser> users, DateTime date, out Dictionary<string, List<int>> dataMapper)
        {
            dataMapper = new Dictionary<string, List<int>>();
            if (debins == null)
                return null;

            List<User> recipients = new List<User>();
            foreach (var debin in debins)
            {
                if(debin.Status == PaymentStatus.Expired) {
                    var user = users.Where(x => x.IdApplicationUser == debin.Payer.Id.ToString()).FirstOrDefault();
                    if (user != null)
                    {
                        var rolCheck = user.Roles.Any(ur => this.Roles.Any(nr => nr.Name == ur.Role));
                        if (rolCheck) {
                            if (dataMapper.TryGetValue(user.IdApplicationUser, out List<int> items))
                                items.Add(debin.Id);
                            else
                                dataMapper.Add(user.IdApplicationUser, new List<int> { debin.Id });
                            if (!recipients.Any(x => x.Id == user.IdApplicationUser))
                                recipients.Add(new User() { Id = user.IdApplicationUser, Email = user.Email });
                        }

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
