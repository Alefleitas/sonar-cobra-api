using Newtonsoft.Json;

namespace nordelta.cobra.service.quotations.Models.OMS
{
    public class Bono
    {
        public string Type { get; set; }
        public string Status { get; set; }
        public MarketData MarketData { get; set; }
        public int Depth { get; set; }
        public bool Aggregated { get; set; }
    }

    public class MarketData
    {
        public LA LA { get; set; }
    }

    //LA: Ultimo Precio
    public class LA
    {
        public double Price { get; set; }
        public int Size { get; set; }
    }
}
