using FiapProcessaVideo.Infrastructure.Messaging.Model;
using Newtonsoft.Json;

namespace FiapLanchonete.Infrastructure.Model
{
    public class PayloadVideoWrapper
    {
        [JsonProperty("pattern")]
        public string Pattern { get; set; }

        [JsonProperty("data")]
        public VideoUploadedEvent Data { get; set; }
    }
}