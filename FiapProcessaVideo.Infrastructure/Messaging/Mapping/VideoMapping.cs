using FiapProcessaVideo.Infrastructure.Messaging.Model;
using FiapProcessaVideo.Infrastructure.Messaging.Mapping;
using FiapProcessaVideo.Domain;

namespace FiapProcessaVideo.Infrastructure.Messaging.Mapping
{
    public class VideoMapping
    {
        public VideoMapping() {}

        public Video ToDomain(VideoUploadedEvent videoUploadedEvent)
        {
            VideoMapping videoMapping = new VideoMapping();

            Video videoDomain = Video.Load(videoUploadedEvent.UserId, videoUploadedEvent.Email, videoUploadedEvent.Phone, videoUploadedEvent.File);
            
            return videoDomain;
        }
    }
}
