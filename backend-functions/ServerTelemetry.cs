using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using System.Runtime.Serialization;

namespace Rahka.Wuma
{
    public enum ServerType
    {
        [EnumMember(Value = "dedicated")]
        Dedicated,

        [EnumMember(Value = "listen")]
        Listen,

        [EnumMember(Value = "singleplayer")]
        Singleplayer
    }

    public class HttpServerTelemetryModel
    {
        [JsonProperty("ip")]
        public string Ip { get; set; }

        [JsonProperty("name")]
        public string Name { get; set; }

        [JsonProperty("type")]
        public ServerType Type { get; set; }
    }

    public class CosmosServerTelemetryModel
    {
        [JsonProperty("id")]
        public string Id { get; set; }

        [JsonProperty("name")]
        public string Name { get; set; }

        [JsonProperty("type")]
        [JsonConverter(typeof(StringEnumConverter))]
        public ServerType Type { get; set; }

        [JsonProperty("ttl")]
        public int TTL { get; set; }

        [JsonProperty("partitionKey")]
        public string PartitionKey => "partitionKey";
    }

}
