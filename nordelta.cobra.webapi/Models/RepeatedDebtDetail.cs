using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Threading.Tasks;

namespace nordelta.cobra.webapi.Models
{
    [Table("RepeatedDebtDetails")]
    public class RepeatedDebtDetail
    {
        [Key]
        public int Id { get; set; }
        [Required]
        public string FileName { get; set; }
        [Required]
        public string NroComprobante { get; set; }
        [Required]
        public string FechaPrimerVenc { get; set; }
        [Required]
        public string CodigoMoneda { get; set; }
        public string CodigoProducto { get; set; }
        public string CodigoTransaccion { get; set; }
        [Required]
        public string RazonSocialCliente { get; set; }
        [Required]
        public string NroCuitCliente { get; set; }
        [Required]
        public string IdClienteOracle { get; set; }
        [Required]
        public string IdSiteOracle { get; set; }
    }
}
