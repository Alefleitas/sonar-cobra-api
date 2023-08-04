using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using nordelta.cobra.webapi.Models;

namespace nordelta.cobra.webapi.Controllers.ViewModels
{
    public class CommunicationViewModel
    {
        public Communication communication { get; set; }
        public string correoElectronico { get; set; }
    }
}
