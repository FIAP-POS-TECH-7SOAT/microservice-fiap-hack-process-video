using FiapProcessaVideo.Application.Model;
using FiapProcessaVideo.Domain;

namespace FiapProcessaVideo.Application.Mapping
{
    public class VideoMapping
    {
        public VideoMapping() {}

        public Video ToDomain(VideoUploadedEvent videoUploadedEvent)
        {
            Video videoDomain = Video.Load(videoUploadedEvent.UserId, videoUploadedEvent.Email, videoUploadedEvent.Phone, videoUploadedEvent.File);
            
            return videoDomain;
        }

        public VideoUploadedEvent ToRabbitMQ(Video video)
        {
            VideoUploadedEvent videoRabbitMq = new VideoUploadedEvent();
            videoRabbitMq.File = video.VideoKey;
            videoRabbitMq.Email = video.Email;
            videoRabbitMq.UserId = video.UserId;
            videoRabbitMq.Id = video.Id.ToString();
            videoRabbitMq.CreatedAt = video.CreatedAt.ToString();
            videoRabbitMq.UpdatedAt = video.UpdateAt.ToString();

            return videoRabbitMq;
        }
    }
}
