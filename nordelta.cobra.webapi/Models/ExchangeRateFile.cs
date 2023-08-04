using System;
using System.ComponentModel.DataAnnotations;

namespace nordelta.cobra.webapi.Models
{
    public class ExchangeRateFile
    {
        [Key]
        public int Id { get; set; }
        public string FileName { get; set; }
        public string UvaExchangeRate { get; set; }
        public string TimeStamp { get; set; }
    }
}
