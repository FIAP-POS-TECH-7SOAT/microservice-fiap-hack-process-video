namespace FiapProcessaVideo.Domain
{
    public sealed class Video
    {
        public Guid Id { get; set; }

        public string UserId { get; private set; }

        public string Email { get; private set; }

        public string Phone { get; private set; }

        // AWS S3 archive video key
        public string VideoKey { get; private set; }

        public DateTime ProcessedAt { get; private set; }

        public string Status { get ; private set; }

        public DateTime CreatedAt { get; set; }

        public DateTime UpdateAt { get; set; }

        public Video(Guid id, string userId, string email, string phone, string videoKey, string status, DateTime processedAt, DateTime createdAt, DateTime updateAt)
        {
            Id = id;
            UserId = userId;
            Email = email;
            Phone = phone;
            VideoKey = videoKey;
            Status = status;
            ProcessedAt = processedAt;
            CreatedAt = createdAt;
            UpdateAt = updateAt;
        }

        public static Video Load(string userId, string email, string phone, string videoKey)
        {
            if (string.IsNullOrWhiteSpace(userId))
            {
                throw new ArgumentException("User ID cannot be null or empty.", nameof(userId));
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
                id: Guid.NewGuid(),
                userId: userId,
                email: email,
                phone: phone,
                videoKey: videoKey,
                status: "Pending", // Default status
                processedAt: DateTime.MinValue, // Set to the minimum value until processed
                createdAt: DateTime.UtcNow,
                updateAt: DateTime.UtcNow
            );

            return video;
        }
    }
}
