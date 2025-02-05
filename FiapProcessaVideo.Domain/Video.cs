namespace FiapProcessaVideo.Domain
{
    public sealed class Video
    {
        public string Id { get; set; }

        public string Email { get; private set; }

        public string Phone { get; private set; }

        // AWS S3 archive video key
        public string VideoKey { get; private set; }

        public DateTime ProcessedAt { get; private set; }

        public string Status { get ; private set; }
        public string Url { get; set; }

        public DateTime CreatedAt { get; set; }

        public DateTime UpdateAt { get; set; }

        public Video(string id, string email, string phone, string videoKey, string status, DateTime processedAt, DateTime createdAt, DateTime updateAt)
        {
            Id = id;
            Email = email;
            Phone = phone;
            VideoKey = videoKey;
            Status = status;
            ProcessedAt = processedAt;
            CreatedAt = createdAt;
            UpdateAt = updateAt;
        }

        public static Video Load(string id, string email, string phone, string videoKey)
        {
            if (string.IsNullOrWhiteSpace(id))
            {
                throw new ArgumentException("Id cannot be null or empty.", nameof(id));
            }

            if (string.IsNullOrWhiteSpace(email))
            {
                throw new ArgumentException("Email cannot be null or empty.", nameof(email));
            }

            if (string.IsNullOrWhiteSpace(phone))
            {
                throw new ArgumentException("Phone cannot be null or empty.", nameof(phone));
            }

            if (string.IsNullOrWhiteSpace(videoKey))
            {
                throw new ArgumentException("Video key cannot be null or empty.", nameof(videoKey));
            }

            // Initialize a new Video instance using the constructor
            var video = new Video(
                id: id,
                email: email,
                phone: phone,
                videoKey: videoKey,
                status: "Waiting", // Default status
                processedAt: DateTime.MinValue, // Set to the minimum value until processed
                createdAt: DateTime.UtcNow,
                updateAt: DateTime.UtcNow
            );

            return video;
        }
    }
}
