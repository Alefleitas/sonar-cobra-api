using System.Collections.Generic;

namespace nordelta.cobra.webapi.Services.DTOs
{
    public class ChequeDto : PaymentMethodDto
    {
        public dynamic ObsLibre { get; set; }
        public IEnumerable<dynamic> ObsLibres { get; set; }
    }
}
