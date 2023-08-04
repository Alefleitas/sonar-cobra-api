using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Diagnostics.CodeAnalysis;

namespace nordelta.cobra.webapi.Models
{
    [Table("AdvanceFees")]
    public class AdvanceFee
    {
        [Key]
        public int Id { get; set; }
        [Required]
        public string CodProducto { get; set; }
        [Required]
        public string UserId { get; set; }
        [Required]
        public long ClientCuit { get; set; }
        [Required]
        public DateTime Vencimiento { get; set; }
        [Required]
        public Currency Moneda { get; set; }
        [Required]
        public float Importe { get; set; }
        [Required]
        public float Saldo { get; set; }
        [Required]
        public EAdvanceFeeStatus Status { get; set; }
        public DateTime CreatedOn { get; set; }
        public bool? Informed { get; set; }
        [NotMapped]
        public string MotivoRechazo { get; set; }

        public bool? AutoApproved { get; set; }

    }

    public enum EAdvanceFeeStatus
    {
        NoSolicitado,
        Pendiente,
        Aprobado,
        Rechazado
    }
}