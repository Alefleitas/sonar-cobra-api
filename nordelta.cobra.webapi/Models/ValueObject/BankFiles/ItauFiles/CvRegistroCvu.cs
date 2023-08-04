using System;
using FileHelpers;
using nordelta.cobra.webapi.Models.ValueObject.BankFiles;

namespace nordelta.cobra.webapi.Models.ValueObject.BankFiles.ItauFiles
{
    [FixedLengthRecord(FixedMode.AllowVariableLength)]
    public class CvRegistroCvu : FileRegistro
    {
        [FieldFixedLength(8)]
        [FieldConverter(ConverterKind.Date, "yyyyMMdd")]
        public DateTime FechaOperacion { get; set; }
        [FieldFixedLength(22)]
        public string TransaccionId { get; set; }
        [FieldFixedLength(11)]
        public string CuitAsociado { get; set; }
        [FieldFixedLength(2)]
        public string FuncionOrigenPsp { get; set; }
        [FieldFixedLength(2)]
        public string Estado { get; set; }
        [FieldFixedLength(40)]
        public string NombreCuit { get; set; }
        [FieldFixedLength(3)]
        public string CodigoMoneda { get; set; }
        [FieldFixedLength(1)]
        public string TipoPersona { get; set; }
        [FieldFixedLength(11)]
        public string ClienteId { get; set; }
        [FieldFixedLength(11)]
        public string CuitEmpresa { get; set; }
        [FieldFixedLength(18)]
        public string CuentaRecaudadoraId { get; set; }
        [FieldFixedLength(22)]
        public string CvuId { get; set; }
        [FieldFixedLength(1)]
        public string TipoCuentaCvu { get; set; }
        [FieldFixedLength(20)]
        public string AliasCuentaCvu { get; set; }
        [FieldFixedLength(26)]
        public string FechaAlta { get; set; }
        [FieldFixedLength(26)]
        public string FechaUltimaActualizacion { get; set; }
        [FieldFixedLength(8)]
        public string FechaRespuestaCoelsa { get; set; }
        [FieldFixedLength(4)]
        public string CodRetCoelsa { get; set; }
        [FieldFixedLength(7)]
        public string MensajeError { get; set; }
        [FieldFixedLength(70)]
        public string DescripcionMensaje { get; set; }
    }
}
