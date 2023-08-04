using Newtonsoft.Json;

namespace nordelta.cobra.service.quotations.Models.OMS
{
    public class Indice
    {
        [JsonProperty("type")]
        public string Type { get; set; }

        [JsonProperty("description")]
        public string Description { get; set; }

        [JsonProperty("name")]
        public string Name { get; set; }

        [JsonProperty("price")]
        public Double Price { get; set; }

        [JsonProperty("highValue")]
        public Double HighValue { get; set; }

        [JsonProperty("minValue")]
        public Double MinValue { get; set; }

        [JsonProperty("variation")]
        public Double Variation { get; set; }

        [JsonProperty("closingPrice")]
        public Double ClosingPrice { get; set; }

    }
}
