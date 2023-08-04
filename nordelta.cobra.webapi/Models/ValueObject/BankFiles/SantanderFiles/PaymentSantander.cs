using System.Collections.Generic;

namespace nordelta.cobra.webapi.Models.ValueObject.BankFiles.SantanderFiles
{
    public class PaymentSantander : FileRegistro
    {
        public HeaderSantander Header { get; set; } 
        public PrSantander Payment { get; set; }
        public IEnumerable<PiSantander> Instruments { get; set; }
        public IEnumerable<PdSantander> Documents { get; set; }
    }
}
