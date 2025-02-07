using FiapProcessaVideo.Application.Model;

namespace FiapProcessaVideo.Application.Messaging.Interfaces
{
    public interface IMessagePublisher{
        void PublishNotificationCreated(PayloadVideoWrapper payloadVideo, string routingKey);
    }
}
