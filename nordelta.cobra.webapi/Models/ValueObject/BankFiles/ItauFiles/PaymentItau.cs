using System.Collections.Generic;

namespace nordelta.cobra.webapi.Models.ValueObject.BankFiles.ItauFiles
{
    public class PaymentItau : FileRegistro
    {
        public List<PiItau> Instruments { get; set; }
    }
}
