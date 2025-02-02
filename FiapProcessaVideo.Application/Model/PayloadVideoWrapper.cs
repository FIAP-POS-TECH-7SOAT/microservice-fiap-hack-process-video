using Newtonsoft.Json;

namespace FiapProcessaVideo.Application.Model
{
    public class PayloadVideoWrapper
    {
        [JsonProperty("pattern")]
        public string Pattern { get; set; }

        [JsonProperty("data")]
        public VideoUploadedEvent Data { get; set; }
    }
}