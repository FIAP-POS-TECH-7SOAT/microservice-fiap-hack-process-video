using FiapProcessaVideo.Application.Model;
using FiapProcessaVideo.Domain;

namespace FiapProcessaVideo.Application.Mapping
{
    public class VideoMapping
    {
        public VideoMapping() {}

        public Video ToDomain(VideoUploadedEvent videoUploadedEvent)
        {
            Video videoDomain = Video.Load(videoUploadedEvent.Id, videoUploadedEvent.Email, videoUploadedEvent.Phone, videoUploadedEvent.File);
            
            return videoDomain;
        }

        public VideoUploadedEvent ToRabbitMQ(Video video)
        {
            VideoUploadedEvent videoRabbitMq = new VideoUploadedEvent();
            videoRabbitMq.File = video.VideoKey;
            videoRabbitMq.Email = video.Email;
            videoRabbitMq.Phone = video.Phone;
            videoRabbitMq.Status = video.Status;
            videoRabbitMq.Url = video.Url;
            videoRabbitMq.Id = video.Id;
            videoRabbitMq.CreatedAt = video.CreatedAt.ToString();
            videoRabbitMq.UpdatedAt = video.UpdateAt.ToString();

            return videoRabbitMq;
        }
    }
}
