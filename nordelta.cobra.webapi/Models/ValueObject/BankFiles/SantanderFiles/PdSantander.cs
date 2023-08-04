using FileHelpers;

namespace nordelta.cobra.webapi.Models.ValueObject.BankFiles.SantanderFiles
{
    [FixedLengthRecord(FixedMode.AllowVariableLength)]
    [IgnoreEmptyLines]
    public class PdSantander
    {
        [FieldFixedLength(17)]
        public string CodigoOrganismo { get; set; } // CuitEmpresa(11) | DigitoEmpresa(1) | CodigoProducto(3) | NroAcuerdo(2)

        [FieldFixedLength(5)]
        public string NroRendicion { get; set; }

        [FieldFixedLength(1)]
        public string TipoRegistro { get; set; }  // D

        [FieldFixedLength(19)]
        public string IdRegistro { get; set; }

        [FieldFixedLength(1)]
        public string NroSecuecia { get; set; }  // 2

        // - - Gral

        [FieldFixedLength(2)]
        public string TipoComprobante { get; set; }

        [FieldFixedLength(15)]
        public string NroComprobante { get; set; }

        [FieldFixedLength(4)]
        public string NroCuota { get; set; }

        [FieldFixedLength(8)]
        public string FechaVto { get; set; }

        [FieldFixedLength(5)]
        public string TasaPunitorios { get; set; }

        [FieldFixedLength(15)]
        public string ImporteCtaComercial { get; set; }

        [FieldFixedLength(15)]
        public string ImportePunitorios { get; set; }

        [FieldFixedLength(15)]
        public string ImporteDescuento { get; set; }

        [FieldFixedLength(15)]
        public string ImportePago { get; set; }

        [FieldFixedLength(18)]
        public string DatoLibre1 { get; set; }

        [FieldFixedLength(15)]
        public string DatoLibre2 { get; set; }

        [FieldFixedLength(15)]
        public string DatoLibre3 { get; set; }

        [FieldFixedLength(215)]
        public string Filler { get; set; }
    }
}
