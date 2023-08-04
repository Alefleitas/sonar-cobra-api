using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace nordelta.cobra.webapi.Services.DTOs
{
    public class UnappliedPaymentDto
    {
        public string Fecha { get; set; }
        public string Importe { get; set; }
        public string Moneda { get; set; }
        public string BuId { get; set; }
        public string DocId { get; set; }
        public string LegalEntityId { get; set; }
        public string Operacion { get; set; }
        public string ImporteTc { get; set; }
        public string Conversion { get; set; }
    }
}
