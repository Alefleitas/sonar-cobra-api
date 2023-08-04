using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace nordelta.cobra.webapi.Controllers.ViewModels
{
    public class ContactViewModel
    {
        public string Name { get; set; }
        public string Tel { get; set; }
        public string Email { get; set; }
        public string Product { get; set; }
        public string Message { get; set; }
        public string EmailTo { get; set; }
        public string Cuit { get; set; }
    }
}
