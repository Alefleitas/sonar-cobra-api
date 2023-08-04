using System;
using FileHelpers;
using nordelta.cobra.webapi.Models.ValueObject.BankFiles;

namespace nordelta.cobra.webapi.Models.ValueObject.BankFiles.ItauFiles
{
    public class MoneyConverter : ConverterBase
    {
        public override object StringToField(string from)
        {
            return Convert.ToDouble(double.Parse(from) / 100);
        }

        public override string FieldToString(object fieldValue)
        {
            return ((double)fieldValue).ToString("#.##").Replace(".", "");
        }

    }

    [FixedLengthRecord(FixedMode.AllowVariableLength)]
    public class PsRegistroCashIn
    {
        [FieldFixedLength(8)]
        [FieldConverter(ConverterKind.Date, "yyyyMMdd")]
        public DateTime FechaOperacion { get; set; }
        [FieldFixedLength(22)]
        public string IdTransaccion { get; set; }
        [FieldFixedLength(22)]
        [FieldOptional]
        public string CbuCredito { get; set; }
        [FieldFixedLength(3)]
        [FieldOptional]
        public string Moneda { get; set; }
        [FieldFixedLength(22)]
        [FieldOptional]
        public string IdCvu { get; set; }
        [FieldFixedLength(11)]
        [FieldOptional]
        public string CuitCvu { get; set; }
        [FieldFixedLength(40)]
        [FieldOptional]
        public string NombreCuitCvu { get; set; }
        [FieldFixedLength(17)]
        [FieldOptional]
        [FieldConverter(typeof(MoneyConverter))]
        public double Importe { get; set; }
        [FieldFixedLength(3)]
        [FieldOptional]
        public string BancoDebito { get; set; }
        [FieldFixedLength(4)]
        [FieldOptional]
        public string SucursalDebito { get; set; }
        [FieldFixedLength(20)]
        [FieldOptional]
        public string AliasDebito { get; set; }
        [FieldFixedLength(22)]
        [FieldOptional]
        public string CbuDebito { get; set; }
        [FieldFixedLength(11)]
        [FieldOptional]
        public string CuitDebito { get; set; }
        [FieldFixedLength(50)]
        [FieldOptional]
        public string NombreDebito { get; set; }
        [FieldFixedLength(2)]
        [FieldOptional]
        public string Estado { get; set; }
    }
}
