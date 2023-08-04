using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace nordelta.cobra.webapi.Services.DTOs
{
    public class AccountBalanceDetailDto
    {
        public int AccountBalanceId { get; set; }

        public DateTime FechaPrimerVencimiento { get; set; }

        public string CodigoMoneda { get; set; }

        public decimal ImportePrimerVenc { get; set; }

        public decimal SaldoActual { get; set; }

        public decimal Intereses { get; set; }
    }
}
