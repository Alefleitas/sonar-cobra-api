using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace nordelta.cobra.webapi.Services.DTOs
{
    public class ExternalValidateBankAccountResponse
    {
        public string DenominacionCuit { get; set; }
        public string Validacion { get; set; }
        public string NroCuenta { get; set; }
        public string Cuit { get; set; }
        public string Moneda { get; set; }
    }
}
