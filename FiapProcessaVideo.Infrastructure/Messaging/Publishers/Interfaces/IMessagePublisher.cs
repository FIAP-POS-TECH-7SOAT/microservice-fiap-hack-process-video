using FiapProcessaVideo.Infrastructure.Messaging.Model;

namespace FiapProcessaVideo.Infrastructure.Messaging.Publishers.Interfaces
{
    public interface IMessagePublisher{
        void PublishNotificationCreated(NotificationCreatedEvent notificationEvent);
    }
}
