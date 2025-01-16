using Newtonsoft.Json;

namespace FiapProcessaVideo.WebApi.Model
{
    public class VideoResponseWrapper
    {
        [JsonProperty("pattern")]
        public string Pattern { get; set; }

        [JsonProperty("data")]
        public VideoResponse Data { get; set; }
    }
}
