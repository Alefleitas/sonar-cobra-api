namespace nordelta.cobra.webapi.Models
{
    public class Cash : PaymentMethod
    {
        public Cash()
        {
            base.OlapMethod = "EF";
            base.Instrument = PaymentInstrument.CASH;
        }
    }
}
