using Newtonsoft.Json;
using System;
using nordelta.cobra.webapi.Models;

namespace nordelta.cobra.webapi.Services.DTOs
{
    [JsonObject(ItemNullValueHandling = NullValueHandling.Include )]
    public class BalanceDetailForAdvanceResponse : ResumenCuentaResponse
    {
        public EAdvanceFeeStatus Status { get; set; }
        public DateTime? CreatedOn { get; set; }
        public string? RequestedByCuit { get; set; }
    }
}