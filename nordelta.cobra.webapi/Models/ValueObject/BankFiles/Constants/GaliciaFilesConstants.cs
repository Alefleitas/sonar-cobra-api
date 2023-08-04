namespace nordelta.cobra.webapi.Models.ValueObject.BankFiles.Constants
{
    public class GaliciaFilesConstants
    {
        public const string Header = "H";

        public const string PG = "PG";

        // Instruments
        public const string
            Cheque_electronico_48_hs = "0",
            Efectivo = "1",
            Cheque_48hs_o_al_dia = "2", // Tambien puede ser Cheque Galicia
            Cheque_de_pago_diferido = "3",
            Transferencia = "4",
            Cheque_Galicia = "5",
            Cheque_electronico_de_pago_diferido = "6",
            Nota_de_credito = "7";

        // Status
        public const string
            Pago_Anulado = "S",
            Pago_No_Anulado = "N";
    }
}
