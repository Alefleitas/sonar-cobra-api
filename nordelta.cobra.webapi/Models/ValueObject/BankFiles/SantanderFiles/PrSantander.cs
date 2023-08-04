using FileHelpers;

namespace nordelta.cobra.webapi.Models.ValueObject.BankFiles.SantanderFiles
{
    [FixedLengthRecord(FixedMode.AllowVariableLength)]
    [IgnoreEmptyLines]
    public class PrSantander
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
        public string NroSecuecia { get; set; }  // 0

        // - - Gral

        [FieldFixedLength(22)]
        public string NroCliente { get; set; }

        [FieldFixedLength(30)]
        public string NombreCliente { get; set; }

        [FieldFixedLength(11)]
        public string CuitCliente { get; set; }

        [FieldFixedLength(8)]
        public string FechaPago { get; set; }

        [FieldFixedLength(8)]
        public string HoraPago { get; set; }

        [FieldFixedLength(3)]
        public string NroBanco { get; set; }

        [FieldFixedLength(2)]
        public string CantDocumentos { get; set; }

        [FieldFixedLength(2)]
        public string CantDocPendientes { get; set; }

        [FieldFixedLength(15)]
        public string TotalPagado { get; set; }

        [FieldFixedLength(15)]
        public string TotalComisionEmpresa { get; set; }

        [FieldFixedLength(15)]
        public string TotalComisionDepositante { get; set; }

        [FieldFixedLength(15)]
        public string CotizacionUsdVen { get; set; }

        [FieldFixedLength(15)]
        public string CotizacionUsdComp { get; set; }

        [FieldFixedLength(3)]
        public string SucursalOrigen { get; set; }

        [FieldFixedLength(19)]
        public string NroBoleta { get; set; }

        [FieldFixedLength(4)]
        public string CodigoTransaccion { get; set; }

        [FieldFixedLength(170)]
        public string Filler { get; set; }
    }
}
