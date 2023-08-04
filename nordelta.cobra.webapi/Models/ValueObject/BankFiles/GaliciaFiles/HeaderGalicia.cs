using FileHelpers;
using System;

namespace nordelta.cobra.webapi.Models.ValueObject.BankFiles.GaliciaFiles
{
    [FixedLengthRecord(FixedMode.AllowVariableLength)]
    [IgnoreEmptyLines]
    public class HeaderGalicia
    {
        [FieldFixedLength(1)]
        [FieldOrder(10)]
        public string CodRegistro { get; set; }

        [FieldFixedLength(10)]
        [FieldOrder(20)]
        public string IdEmpresa { get; set; }

        [FieldFixedLength(8)]
        [FieldConverter(ConverterKind.Date, "yyyyMMdd")]
        [FieldOrder(30)]
        public DateTime Fecha { get; set; }

        [FieldFixedLength(6)]
        [FieldOrder(40)]
        public string Hora { get; set; }

        [FieldFixedLength(4)]
        [FieldOrder(50)]
        public string NroArchivo { get; set; }

        [FieldFixedLength(271)]
        [FieldOrder(60)]
        public string Filler { get; set; }
    }
}
