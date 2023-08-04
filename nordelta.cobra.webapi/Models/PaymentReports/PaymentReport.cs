using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using nordelta.cobra.webapi.Models.ArchivoDeuda;

namespace nordelta.cobra.webapi.Models
{
    public class PaymentReport
    {
        [Key]
        public int Id { get; set; }
        public string PayerId { get; set; }
        public DateTime ReportDate { get; set; }
        public string Cuit { get; set; }
        public Currency Currency { get; set; }
        public double Amount { get; set; }
        public List<DetalleDeuda> Debts { get; set; }
        public string Product { get; set; }
        public PaymentReportStatus Status { get; set; }
        public PaymentInstrument Instrument { get; set; }
        public DateTime? ReportDateVto { get; set; }
    }
}
