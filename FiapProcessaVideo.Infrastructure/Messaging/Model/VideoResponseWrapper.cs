using Newtonsoft.Json;

namespace FiapProcessaVideo.Infrastructure.Messaging.Model
{
    public class VideoResponseWrapper
    {
        [JsonProperty("pattern")]
        public string Pattern { get; set; }

        [JsonProperty("data")]
        public VideoResponse Data { get; set; }
    }
}
