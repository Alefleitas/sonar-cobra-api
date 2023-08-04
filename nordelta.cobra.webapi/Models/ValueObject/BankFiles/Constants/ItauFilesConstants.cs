
namespace nordelta.cobra.webapi.Models.ValueObject.BankFiles.Constants
{
    public class ItauFilesConstants
    {
        public const string HeaderCVToken = "HCV";
        public const string HeaderPSToken = "HPS";

        public const string HeaderPCToken = "HCD";
        public const string HeaderNOToken = "HNO";

        public const string PSFile = "PS";
        public const string CVFile = "CV";

        public const string CDFile = "CD";
        public const string NOFile = "NO";

        // Instruments
        public const string
            EFECTIVO = "01",
            DEB_EN_CTA = "02",
            DEBITO_CUENTA_OTRO_BCO = "03",
            CHEQUE_ITAU = "04",
            CHEQUE_OTRO_BCO = "05",
            CPD_ITAU = "06",
            CPD_OTRO_BCO = "07",
            CFU_ITAU = "08",
            CFU_OTRO_BCO = "09",
            CFU_CPD_ITAU = "10",
            CFU_CPD_OTRO_BCO = "11",
            DEBITO_DIRECTO_ITAU = "12",
            DEBITO_DIRECTO_OTRO_BCO = "13",
            TRANSFERENCIAS_OTRO_BCO = "19",
            ECHEQ_AL_DIA = "29", // ECHEQ
            ECHEQ_CPD = "31"; // ECHEQ

        // Status
        public const string
            ACREDITADO = "AC", // Approbed
            CUSTODIA = "CU",
            DIFERIDO = "DI",
            DIFERIMIENTO_MANUAL = "DM",
            DIFERIDO_AUTOMATICO = "DT",
            REVERSADO = "ES",
            INGRESADO = "IN",
            PENDIENTE_DE_ACRED = "PA",
            RECHAZADO = "RC", // Rejected
            RESCATADO = "RE",
            ANULADO = "AN", // CANCELADO
            PENDIENTE_DE_ACREDITACION_POR_REDEPOSITO = "PR";

    }
}
