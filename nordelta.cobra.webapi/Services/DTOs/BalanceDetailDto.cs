using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace nordelta.cobra.webapi.Services.DTOs
{
    public class BalanceDetailDto
    {
        public string BusinessUnitName { get; set; }
        public string BuId { get; set; }
        public string AccountName { get; set; }
        public string CustomerDocumentType { get; set; }
        public string CustomerDocumentNumber { get; set; }
        public string RpAccountName { get; set; }
        public string RpCustomerDocumentType { get; set; }
        public string RpCustomerDocumentNumber { get; set; }
        public string ReferenciaCliente { get; set; }
        public string ReferenciaDomicilioCliente { get; set; }
        public string Producto { get; set; }
        public string PsType { get; set; }
        public string DocType { get; set; }
        public string TrxNumber { get; set; }
        public string TrxDate { get; set; }
        public string DueDate { get; set; }
        public string NroCuota { get; set; }
        public string CurrencyCode { get; set; }
        public string AmounDueOriginal { get; set; }
        public string AmountAdjusted { get; set; }
        public string AmountDueRemaining { get; set; }
        public string DueDays { get; set; }
        public string ProdName { get; set; }
        public string AmountLineItemsRemaining { get; set; }
        public string TrxId { get; set; }
        public string FacElect { get; set; }
        public string PublishDebt { get; set; }
        public string OperationType { get; set; }
    }
}
