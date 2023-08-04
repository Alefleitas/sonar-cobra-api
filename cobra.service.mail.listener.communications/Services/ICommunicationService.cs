using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using cobra.service.mail.listener.communications.Models;

namespace cobra.service.mail.listener.communications.Services
{
    public interface ICommunicationService
    {

        Task PostCommunication(ICollection<Communication> communications);
    }
}
