using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace nordelta.cobra.webapi.Services.DTOs
{
    public class PaymentHistoryDetailDto
    {
        public string Tipo { get; set; }
        public string Fecha { get; set; }
        public string Importe { get; set; }
        public string Moneda { get; set; }
        public string ImporteFC { get; set; }
        public string MonedaFC { get; set; }
        public string ApplicationType { get; set; }
        public string BuId { get; set; }
        public string DocId { get; set; }
        public string DocNumber { get; set; }
        public string LegalEntityId { get; set; }
        public string TrxId { get; set; }
        public string FacElect { get; set; }
        public string TrxNumber { get; set; }
        public string ApplTc { get; set; }
        public string DocTc { get; set; }
    }
}
