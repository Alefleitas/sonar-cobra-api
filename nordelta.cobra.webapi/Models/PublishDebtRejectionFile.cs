using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace nordelta.cobra.webapi.Models
{
    public class PublishDebtRejectionFile
    {
        [Key]
        public int Id { get; set; }
        public string FileName { get; set; }
        public DateTime FileDate { get; set; }

        public List<PublishDebtRejection> PublishDebtRejections { get; set; }
    }
}
