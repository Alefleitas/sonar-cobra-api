namespace nordelta.cobra.webapi.Models.ValueObject.BankFiles.GaliciaFiles
{
    public class PaymentGalicia : FileRegistro
    {
        public HeaderGalicia Header { get; set; }
        public PrGalicia Pago { get; set; }
    }
}
