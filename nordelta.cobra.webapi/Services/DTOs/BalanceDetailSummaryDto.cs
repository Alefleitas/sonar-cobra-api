using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace nordelta.cobra.webapi.Services.DTOs
{
    public class BalanceDetailSummaryDto
    {
        public string Cuit { get; set; }
        public string Product { get; set; }
        public string ClientReference { get; set; }
        public string RazonSocial { get; set; }
        public string CurrencyCode { get; set; }
        public int Quantity { get; set; }
        public decimal TotalAmountDueRemaining { get; set; }
        public int OverdueQuantity { get; set; }
        public decimal TotalAmountOverdue { get; set; }
        public string OverdueDate { get; set; }
        public int FutureQuantity { get; set; }
        public decimal TotalAmountFuture { get; set; }
        public string PublishDebt { get; set; }

    }
}
