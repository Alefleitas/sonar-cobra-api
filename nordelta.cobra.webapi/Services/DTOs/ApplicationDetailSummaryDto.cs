using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace nordelta.cobra.webapi.Services.DTOs
{
    public class ApplicationDetailSummaryDto
    {
        public string Cuit { get; set; }
        public string Product { get; set; }
        public string CurrencyCode { get; set; }
        public int Quantity { get; set; }
        public decimal TotalAmountApplied { get; set; }
        public string RazonSocial { get; set; }
        public string ClientReference { get; set; }
    }
}
