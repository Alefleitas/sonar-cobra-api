
namespace nordelta.cobra.webapi.Models
{
    public class Echeq : PaymentMethod
    {
        public Echeq()
        {
            base.OlapMethod = "EC";
            base.Instrument = PaymentInstrument.ECHEQ;
        }
    }
}
