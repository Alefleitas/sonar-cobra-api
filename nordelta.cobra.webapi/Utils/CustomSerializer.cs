using Newtonsoft.Json;
using RestSharp;
using RestSharp.Serialization;

namespace nordelta.cobra.webapi.Utils
{
    public class CustomSerializer : IRestSerializer
    {
        public string Serialize(object obj) => JsonConvert.SerializeObject(obj, new Newtonsoft.Json.Converters.StringEnumConverter());

        public string Serialize(Parameter bodyParameter) => Serialize(bodyParameter.Value);

        public T Deserialize<T>(IRestResponse response) => JsonConvert.DeserializeObject<T>(response.Content);

        public string[] SupportedContentTypes { get; } =
        {
            "application/json", "text/json", "text/x-json", "text/javascript", "*+json"
        };

        public string ContentType { get; set; } = "application/json";

        public DataFormat DataFormat { get; } = DataFormat.Json;
    }
}
