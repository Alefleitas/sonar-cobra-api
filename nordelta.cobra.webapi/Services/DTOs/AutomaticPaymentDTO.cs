using nordelta.cobra.webapi.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace nordelta.cobra.webapi.Services.DTOs
{
    public class AutomaticPaymentDTO
    {
        public string PayerId { get; set; }

        public int BankAccountId { get; set; }

        public Currency Currency { get; set; }

        public string Product { get; set; }

    }
}
