using Newtonsoft.Json;

namespace FiapProcessaVideo.Infrastructure.Messaging.Model.Shared
{
    public class FileQueue
    {
        [JsonProperty("name")]
        public string Name { get; set; }

        [JsonProperty("routing_keys")]
        public string[] RoutingKeys { get; set; }
    }
}
