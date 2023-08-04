using FileHelpers;
using System;

namespace nordelta.cobra.webapi.Models.ValueObject.BankFiles.GaliciaFiles
{
    [FixedLengthRecord(FixedMode.AllowVariableLength)]
    public class PrGalicia
    {
        [FieldFixedLength(1)]
        public string CodRegistro { get; set; }

        [FieldFixedLength(4)]
        public string IdTipoCliente { get; set; }

        [FieldFixedLength(15)]
        public string IdCliente { get; set; }

        [FieldFixedLength(18)]
        public string Cuit { get; set; }

        [FieldFixedLength(2)]
        public string TipoDocumento { get; set; } // FC = Factura | PC = Pago de Cuenta | ND = Nota débito | RE = Cheque rechazado | VS = Varios Débito | NC = Nota de crédito

        [FieldFixedLength(25)]
        public string IdDocumento { get; set; } // En PC siempre vacio

        [FieldFixedLength(30)]
        public string IdDocumentoInterno { get; set; }

        [FieldFixedLength(6)]
        public string Division { get; set; }

        [FieldFixedLength(2)]
        public string Moneda { get; set; } // 00 = ARS | 02 = USD

        [FieldFixedLength(8)]
        [FieldConverter(ConverterKind.Date, "yyyyMMdd")]
        public DateTime FechaPago { get; set; }

        [FieldFixedLength(3)]
        public string SucursalPago { get; set; }

        [FieldFixedLength(1)]
        public string FormaPago { get; set; }

        [FieldFixedLength(9)]
        public string IdPago { get; set; }

        [FieldFixedLength(1)]
        public string PagoParcial { get; set; } // S =  el importe pagado por el cliente es menor al importe declarado por la empresa como deuda. | Espacio en blaco

        [FieldFixedLength(15)]
        public string ImportePago { get; set; } // 13 enteros y 2 decimales

        [FieldFixedLength(9)]
        public string NroCheque { get; set; }

        [FieldFixedLength(8)]
        public string CreditingDate { get; set; }

        [FieldFixedLength(15)]
        public string ImporteCheque { get; set; }

        [FieldFixedLength(3)]
        public string CodBanco { get; set; }

        [FieldFixedLength(1)]
        public string PagoInformativo { get; set; } // Si ya fue informado = S | N primer informe

        [FieldFixedLength(1)]
        public string PagoAnulado { get; set; } // S / N

        [FieldFixedLength(25)]
        public string NroDocumentoPago { get; set; }

        [FieldFixedLength(2)]
        public string TipoCanal { get; set; }

        [FieldFixedLength(14)]
        public string DescripcionCanal { get; set; }

        [FieldFixedLength(9)]
        public string NroBoleta { get; set; }

        [FieldFixedLength(73)]
        public string Filler { get; set; }
    }
}
