namespace nordelta.cobra.webapi.Models
{
    public class Cheque : PaymentMethod
    {
        public Cheque()
        {
            base.OlapMethod = "CH";
            base.Instrument = PaymentInstrument.CHEQUE;
        }
    }
}
