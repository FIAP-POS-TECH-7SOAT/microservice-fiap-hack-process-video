using FiapLanchonete.Infrastructure.Model;
using FiapProcessaVideo.Infrastructure.Messaging.Model;

namespace FiapProcessaVideo.Infrastructure.Messaging.Publishers.Interfaces
{
    public interface IMessagePublisher{
        void PublishNotificationCreated(PayloadVideoWrapper payload, string status);
    }
}
