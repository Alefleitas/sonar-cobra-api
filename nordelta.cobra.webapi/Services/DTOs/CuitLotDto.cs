using System.Collections.Generic;

namespace nordelta.cobra.webapi.Services.DTOs
{
    public class CuitLotDto
    {
        public List<string> Cuits { get; set;}
        public bool AreForeigners { get; set;}
    }
}
