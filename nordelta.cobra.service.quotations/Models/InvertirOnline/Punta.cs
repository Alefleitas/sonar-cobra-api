using Newtonsoft.Json;

namespace nordelta.cobra.service.quotations.Models.InvertirOnline
{
    public class Punta
    {
        [JsonProperty("cantidadCompra")]
        public double CantidadCompra { get; set; }

        [JsonProperty("precioCompra")]
        public double PrecioCompra { get; set; }

        [JsonProperty("precioVenta")]
        public double PrecioVenta { get; set; }

        [JsonProperty("cantidadVenta")]
        public double CantidadVenta { get; set; }
    }
}
