namespace nordelta.cobra.webapi.Models.ValueObject.BankFiles.Constants
{
    public class SantanderFilesConstants
    {
        public const string Header = "C";
        public const string PR = "PR"; // Pago Registro
        public const string PI = "PI"; // Instrumento Pago Registro
        public const string PD = "PD"; // Documento Pago Registro

        public const string TypeHeader = "C0";
        public const string TypePR = "D0"; // Pago Registro
        public const string TypePI = "D1"; // Instrumento Pago Registro
        public const string TypePD = "D2"; // Documento Pago Registro
        public const string TypeTrailer = "T9";

        public const int IndexNSR = 42;   // Index Número de secuencia del Registro 43
        public const int IndexTR = 22;    // Index Tipo de Registro 23  

        public const string EFECPESOS = "PESOS";
        public const string EFECUSD = "USD";
        public const string CHEQ = "CHEQUE";
    }
}
