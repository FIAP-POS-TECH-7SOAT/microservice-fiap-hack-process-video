using Newtonsoft.Json;

namespace FiapProcessaVideo.WebApi.Model
{
    public class VideoResponse
    {
        [JsonProperty("video_path")]  // Maps to 'video_path' in JSON
        public string VideoFilePath { get; set; }

        [JsonProperty("duration")]  // Maps to 'duration' in JSON
        public TimeSpan Duration { get; set; }

        [JsonProperty("snapshot_interval")]  // Maps to 'snapshot_interval' in JSON
        public TimeSpan SnapshotInterval { get; set; }

        public VideoResponse() { }
    }
}
