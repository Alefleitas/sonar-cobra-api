using System.Collections.Generic;

namespace nordelta.cobra.webapi.Services.DTOs
{
    public class CashDto : PaymentMethodDto
    {
        public dynamic ObsLibre { get; set; }
        public IEnumerable<dynamic> ObsLibres { get; set; }
    }
}
