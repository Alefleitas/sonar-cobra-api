using System.Collections.Generic;

namespace nordelta.cobra.webapi.Services.DTOs
{
    public class InformDebinPagination
    {
        public InformDebinPagination()
        {
            DebinList = new List<PaymentMethodDto>();
        }
        public List<PaymentMethodDto> DebinList { get; set; }
        public int TotalCount { get; set; }

    }
}
