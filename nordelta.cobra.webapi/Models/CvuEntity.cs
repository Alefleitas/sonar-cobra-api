using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace nordelta.cobra.webapi.Models
{
    public class CvuEntity
    {
        [Key]
        public int Id { get; set; }

        [Required]
        public string ItauCreationTransactionId { get; set; }
        public string CvuValue { get; set; }
        public string Alias { get; set; }
        public Currency Currency { get; set; }
        [Required]
        public int AccountBalanceId { get; set; }
        [ForeignKey("AccountBalanceId")]
        public virtual AccountBalance AccountBalance { get; set; }

        public CvuEntityStatus Status { get; set; }
        public DateTime CreationDate { get; set; }

        public virtual List<CvuOperation> CvuOperations { get; set; }

    }
}
