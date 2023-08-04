using FileHelpers;

namespace nordelta.cobra.webapi.Models.ValueObject.BankFiles.SantanderFiles
{
    [FixedLengthRecord(FixedMode.AllowVariableLength)]
    [IgnoreEmptyLines]
    public class PiSantander
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
        public string NroSecuecia { get; set; }  // 1

        // - - Gral

        [FieldFixedLength(8)]
        public string DescripcionFormaPago { get; set; }

        [FieldFixedLength(31)]
        public string NroInstrumento { get; set; }

        [FieldFixedLength(15)]
        public string Importe { get; set; }

        [FieldFixedLength(8)]
        public string FechaAcreditacion { get; set; }

        [FieldFixedLength(1)]
        public string MarcaAcreditacion { get; set; }

        [FieldFixedLength(8)]
        public string FechaVtoCPD { get; set; }

        [FieldFixedLength(2)]
        public string CodigoFormaPago { get; set; }

        [FieldFixedLength(3)]
        public string CodigoMotivoRechazo { get; set; }

        [FieldFixedLength(1)]
        public string CodigoCambioDatos { get; set; }

        [FieldFixedLength(1)]
        public string CambioEstadoAcreditacion { get; set; }

        [FieldFixedLength(1)]
        public string IndicadorPraRedicion { get; set; }

        [FieldFixedLength(2)]
        public string NroSecInstrumento { get; set; }

        [FieldFixedLength(276)]
        public string Filler { get; set; }
    }
}
