using System;
using FileHelpers;

namespace nordelta.cobra.webapi.Models.ValueObject.BankFiles.ItauFiles
{
    [FixedLengthRecord(FixedMode.AllowVariableLength)]
    [IgnoreEmptyLines]
    public class HeaderItau
    {
        [FieldFixedLength(11)]
        [FieldOrder(10)]
        public string Cuit { get; set; }

        [FieldFixedLength(3)]
        [FieldOrder(20)]
        public string Producto { get; set; }
        [FieldFixedLength(6)]
        [FieldOrder(30)]
        public string Convenio { get; set; }
        [FieldFixedLength(5)]
        [FieldOrder(40)]
        public string NumRendicion { get; set; }
        [FieldFixedLength(8)]
        [FieldConverter(ConverterKind.Date, "yyyyMMdd")]
        [FieldOrder(50)]
        public DateTime FechaGeneracionArchivo { get; set; }
        [FieldFixedLength(6)]
        [FieldOrder(60)]
        public string Hora { get; set; }
        [FieldFixedLength(1)]
        [FieldOrder(70)]
        public string TipoRegistro { get; set; }
        [FieldFixedLength(2)]
        [FieldOrder(80)]
        public string IdRendicion { get; set; }
        [FieldFixedLength(6)]
        [FieldOrder(90)]
        public string NroServicioPyC { get; set; }
    }
}
