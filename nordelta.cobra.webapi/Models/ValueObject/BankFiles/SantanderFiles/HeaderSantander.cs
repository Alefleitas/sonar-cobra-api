using FileHelpers;

namespace nordelta.cobra.webapi.Models.ValueObject.BankFiles.SantanderFiles
{
    [FixedLengthRecord(FixedMode.AllowVariableLength)]
    [IgnoreEmptyLines]
    public class HeaderSantander
    {
        [FieldFixedLength(17)]
        [FieldOrder(10)]
        public string CodigoOrganismo { get; set; } // CuitEmpresa(11) | DigitoEmpresa(1) | CodigoProducto(3) | NroAcuerdo(2)

        [FieldFixedLength(5)]
        [FieldOrder(20)]
        public string NroRendicion { get; set; }

        [FieldFixedLength(1)]
        [FieldOrder(30)]
        public string TipoRegistro { get; set; }  // C

        [FieldFixedLength(19)]
        [FieldOrder(40)]
        public string IdRegistro { get; set; }

        [FieldFixedLength(1)]
        [FieldOrder(50)]
        public string NroSecuecia { get; set; }  // 0

        // - - Gral

        [FieldFixedLength(8)]
        [FieldOrder(60)]
        public string FechaRendicion { get; set; }

        [FieldFixedLength(30)]
        [FieldOrder(70)]
        public string RazonSocialEmpresa { get; set; } // Nombre Empresa | Metodo Pago

        [FieldFixedLength(4)]
        [FieldOrder(80)]
        public string CodigoPais { get; set; } // Código de país de la cuenta Recaudadora

        [FieldFixedLength(1)]
        [FieldOrder(90)]
        public string CodigoMoneda { get; set; } // Recaudadora

        [FieldFixedLength(4)]
        [FieldOrder(100)]
        public string CodigoEntidad { get; set; } // Recaudadora

        [FieldFixedLength(4)]
        [FieldOrder(110)]
        public string CodigoSucursal { get; set; } // Recaudadora

        [FieldFixedLength(1)]
        [FieldOrder(120)]
        public string DvCbuB1 { get; set; } // Dígito verificador del bloque 1 de la CBU

        [FieldFixedLength(3)]
        [FieldOrder(130)]
        public string FijoCbu2 { get; set; } // Fijo Siempre 000

        [FieldFixedLength(1)]
        [FieldOrder(140)]
        public string TipoCuenta { get; set; }  // Recaudadora

        [FieldFixedLength(1)]
        [FieldOrder(150)]
        public string MonedaCuenta { get; set; } // Recaudadora

        [FieldFixedLength(11)]
        [FieldOrder(160)]
        public string NroCuenta { get; set; } // Recaudadora

        [FieldFixedLength(1)]
        [FieldOrder(170)]
        public string DvCbuB2 { get; set; } // Dígito verificador del bloque 2 de la CBU

        [FieldFixedLength(288)]
        [FieldOrder(180)]
        public string Filler { get; set; }
    }
}
