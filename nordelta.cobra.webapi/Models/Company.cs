using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace nordelta.cobra.webapi.Models
{
    public class Company
    {
        [Key]
        public int Id { get; set; }
        public string SocialReason { get; set; }
        public string Cuit { get; set; }
        public string CbuPeso { get; set; }
        public string CbuDolar { get; set; }
        public string ConvenioPeso { get; set; }
        public string ConvenioDolar { get; set; }
    }
}
