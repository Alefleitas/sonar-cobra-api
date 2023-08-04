using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace nordelta.cobra.webapi.Models
{
    public class PublishDebtRejection
    {
        [Key]
        public int Id { get; set; }
        public int PublishDebtRejectionFileId { get; set; }
        public string Empresa { get; set; }
        public int UltimaRendicionProcesada { get; set; }
        public string NroCliente { get; set; }
        public string CuitCliente { get; set; }
        public string Moneda { get; set; }
        public int NroCuota { get; set; }
        public string TipoComprobante { get; set; }
        public string NroComprobante { get; set; }
        public List<PublishDebtRejectionError> Errors { get; set; }

        [ForeignKey("PublishDebtRejectionFileId")]
        public PublishDebtRejectionFile ArchivoRechazo { get; set; }
    }

    public class PublishDebtRejectionError
    {
        [Key]
        public int Id { get; set; }
        public int CodigoError { get; set; }
        public string DescripcionError { get; set; }
        public int PublishDebtRejectionId { get; set; }

        [ForeignKey("PublishDebtRejectionId")]
        public PublishDebtRejection PublishDebtRejection { get; set; }
    }
}
