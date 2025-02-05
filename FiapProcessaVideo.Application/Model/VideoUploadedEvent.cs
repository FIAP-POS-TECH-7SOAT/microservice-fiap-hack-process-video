using Newtonsoft.Json;

namespace FiapProcessaVideo.Application.Model
{
    public class VideoUploadedEvent
    {
        [JsonProperty("id")]
        public string Id { get; set; }
        
        [JsonProperty("file")]
        public string File { get; set; }        
        
        [JsonProperty("user_id")]
        public string UserId { get; set; }

        [JsonProperty("email")]
        public string Email { get; set; }

        [JsonProperty("phone")]
        public string Phone { get; set; }

        [JsonProperty("status")]
        public string Status { get; set; }

        [JsonProperty("url")]
        public string Url { get; set; }

        [JsonProperty("created_at")]
        public string CreatedAt { get; set; }

        [JsonProperty("updated_at")]
        public string UpdatedAt { get; set; }

        public VideoUploadedEvent() { }
    }
}
