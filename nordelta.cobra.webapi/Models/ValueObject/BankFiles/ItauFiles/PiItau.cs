using FileHelpers;

namespace nordelta.cobra.webapi.Models.ValueObject.BankFiles.ItauFiles
{
    [FixedLengthRecord(FixedMode.AllowVariableLength)]
    public class PiItau
    {
        [FieldFixedLength(11)]
        public string CuitEmpresa { get; set; }

        [FieldFixedLength(3)]
        public string NroProducto { get; set; } // 100

        [FieldFixedLength(6)]
        public string NroConvenio { get; set; }

        [FieldFixedLength(5)]
        public string NroRendicion { get; set; }

        [FieldFixedLength(14)]
        public string FechaArchivo { get; set; }

        [FieldFixedLength(1)]
        public string TipoRegistro { get; set; }

        [FieldFixedLength(22)]
        public string IdCliente { get; set; }

        [FieldFixedLength(2)]
        public string TipoDocumento { get; set; }

        [FieldFixedLength(11)]
        public string NroDocumento { get; set; }

        [FieldFixedLength(11)]
        public string CuitCliente { get; set; }

        [FieldFixedLength(19)]
        public string IdOperacion { get; set; }

        [FieldFixedLength(2)]
        public string CodInstrumento { get; set; }

        [FieldFixedLength(29)]
        public string IdInstrumento { get; set; }

        [FieldFixedLength(5)]
        public string SecInstrumento { get; set; }

        [FieldFixedLength(8)]
        public string DesInstrumento { get; set; }

        [FieldFixedLength(17)]
        public string Importe { get; set; }

        [FieldFixedLength(8)]
        public string FechaEmision { get; set; }

        [FieldFixedLength(8)]
        public string FechaPago { get; set; }

        [FieldFixedLength(8)]
        public string FechaAcreditacion { get; set; }

        [FieldFixedLength(8)]
        public string FechaDisposicion { get; set; }

        [FieldFixedLength(2)]
        public string TipoDocRelacionado1 { get; set; }

        [FieldFixedLength(11)]
        public string NroDocRelacionado1 { get; set; }

        [FieldFixedLength(2)]
        public string TipoDocRelacionado2 { get; set; }

        [FieldFixedLength(11)]
        public string NroDocRelacionado2 { get; set; }

        [FieldFixedLength(2)]
        public string TipoDocRelacionado3 { get; set; }

        [FieldFixedLength(11)]
        public string NroDocRelacionado3 { get; set; }

        [FieldFixedLength(10)]
        public string CampoModificado { get; set; }

        [FieldFixedLength(8)]
        public string FechaDiferimiento { get; set; }

        [FieldFixedLength(3)]
        public string CodEstado { get; set; }

        [FieldFixedLength(8)]
        public string DesEstado { get; set; }

        [FieldFixedLength(8)]
        public string FechaEstado { get; set; }

        [FieldFixedLength(8)]
        public string MotivoRechazo { get; set; }

        [FieldFixedLength(1)]
        public string Reversa { get; set; }

        [FieldFixedLength(517)]
        public string FILLER { get; set; }
    }
}
