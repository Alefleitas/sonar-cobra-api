using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace nordelta.cobra.webapi.Controllers.ViewModels
{
    public class QuotationOracleViewModel
    {
        public DateTime DateFrom { get; set; }
        public DateTime DateTo { get; set; }
        public string FromCurrency { get; set; }
        public string ToCurrency { get; set; }
        public string RateType { get; set; }
        public double Rate { get; set; }

        public QuotationOracleViewModel DeepCopy()
        {
            return (QuotationOracleViewModel)this.MemberwiseClone();
        }
    }
}
