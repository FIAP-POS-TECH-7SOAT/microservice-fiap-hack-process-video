using Newtonsoft.Json;

namespace FiapProcessaVideo.Infrastructure.Messaging.Model
{
    public class NotificationCreatedEvent
    {
        [JsonProperty("id")]
        public string Id { get; set; }
        
        [JsonProperty("file")] //filekey from S3 AWS.
        public string File { get; set; }        
        
        [JsonProperty("user_id")]
        public string UserId { get; set; }

        [JsonProperty("email")]
        public string Email { get; set; }
        
        [JsonProperty("status")]
        public string Status { get; set; }

        public NotificationCreatedEvent() { }
    }
}
