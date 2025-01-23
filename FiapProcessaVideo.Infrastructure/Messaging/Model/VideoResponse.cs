using Newtonsoft.Json;

namespace FiapProcessaVideo.Infrastructure.Messaging.Model
{
    public class VideoResponse
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

        [JsonProperty("created_at")]
        public DateTime CreatedAt { get; set; }

        [JsonProperty("updated_at")]
        public DateTime UpdatedAt { get; set; }

        public VideoResponse() { }
    }
}
