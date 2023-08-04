using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using CsvHelper.Configuration.Attributes;

namespace nordelta.cobra.webapi.Services.DTOs
{
    public class BankAccountFile
    {
        [Index(0)]
        public string CompanyName { get; set; }
        
        [Index(1)]
        public string SocialReason { get; set; }
        
        [Index(2)]
        public string ClientCuit { get; set; }
        
        [Index(3)]
        public string Cuit { get; set; }
        
        [Index(4)]
        public string Cbu { get; set; }

        [Index(5)]
        public int Currency { get; set; }
    }
}
