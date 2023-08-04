using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace nordelta.cobra.webapi.Services.DTOs
{
    public class ApplicationDetailDto
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
        public string AmounDueOriginal { get; set; }
        public string AmountAdjusted { get; set; }
        public string AmountDueRemaining { get; set; }
        public string ProdName { get; set; }
        public string CustTrxTypeName { get; set; }
        public string InvoiceCurrencyCode { get; set; }
        public string ApplicationType { get; set; }
        public string DocNumber { get; set; }
        public string DocCurrencyCode { get; set; }
        public string ApplyDate { get; set; }
        public string AmountApplied { get; set; }
        public string AmountAppliedFrom { get; set; }
        public string DocId { get; set; }
        public string LegalEntityId { get; set; }
        public string Amount { get; set; }
        public string DocDate { get; set; }
        public string TrxId { get; set; }
        public string FacElect { get; set; }
        public string OperationType { get; set; }
        public string DocTc { get; set; }
        public string ApplTc { get; set; }
        public string ApplRcvUsd { get; set; }
    }
}
