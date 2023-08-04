using nordelta.cobra.webapi.Models;
using System;
using System.Collections.Generic;

namespace nordelta.cobra.webapi.Services.DTOs
{
    public class PaymentReportDto
    {
        public DateTime ReportDate { get; set; }
        public string Cuit { get; set; }
        public int Currency { get; set; }
        public double Amount { get; set; }
        public List<int> DebtIds { get; set; }
        public PaymentInstrument Type { get; set; }
        public string Product { get; set; }
    }
    
    public class PaymentReportCommandResponseDto
    {
        public string Message { get; set; }
    }
}
