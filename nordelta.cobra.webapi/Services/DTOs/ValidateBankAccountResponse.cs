using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using nordelta.cobra.webapi.Models;

namespace nordelta.cobra.webapi.Services.DTOs
{
    public class ValidateBankAccountResponse
    {
        public string DenominacionCuit { get; set; }
        public string Validacion { get; set; }
        public string NroCuenta { get; set; }
        public string Cuit { get; set; }
        public Currency Currency { get; set; }
    }
}
