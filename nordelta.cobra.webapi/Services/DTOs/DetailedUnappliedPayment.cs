using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace nordelta.cobra.webapi.Services.DTOs
{
    public class DetailedUnappliedPayment
	{
		public string LegalEntityId { get; set; }
		public string BusinessUnitName { get; set; }
		public string BuId { get; set; }
		public string AccountName { get; set; }
		public string ReferenciaCliente { get; set; }
		public string CustomerDocumentType { get; set; }
		public string CustomerDocumentNumber { get; set; }
		public string Producto { get; set; }
		public string DocType { get; set; }
		public string DocId { get; set; }
		public string DocNumber { get; set; }
		public string DocDate { get; set; }
		public string AmountUnapplied { get; set; }
		public string Amount { get; set; }
		public string CurrencyCode { get; set; }
        public string PddRate { get; set; }
    }
}
