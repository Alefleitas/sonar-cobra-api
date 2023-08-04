using System;
using System.Collections.Generic;
using nordelta.cobra.webapi.Models;

namespace nordelta.cobra.webapi.Controllers.ViewModels
{
    public class PaymentReportViewModel
    {
        public DateTime ReportDate { get; set; }
        public string PayerId { get; set; }
        public string Cuit { get; set; }
        public string RazonSocial { get; set; }
        public int Currency { get; set; }
        public double Amount { get; set; }
        public List<int> DebtIds { get; set; }
        public PaymentInstrument Type { get; set; }
        public string Product { get; set; }
        public PaymentReportStatus Status { get; set; }
        public DateTime ReportDateVto { get; set; }
    }
}
