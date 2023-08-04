using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace nordelta.cobra.webapi.Services.DTOs
{
    public class UpdatePublishDebtRequest
    {
        public string Cuit { get; set; }
        public string ProductCode { get; set; }
        public string PublishDebt { get; set; }
    }
}
