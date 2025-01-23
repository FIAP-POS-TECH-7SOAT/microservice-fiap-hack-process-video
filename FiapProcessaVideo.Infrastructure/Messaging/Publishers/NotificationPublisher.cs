using FiapProcessaVideo.Infrastructure.Messaging.Publishers.Interfaces;
using FiapProcessaVideo.Infrastructure.Messaging.Model;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using Microsoft.Extensions.Hosting;
using Newtonsoft.Json;
using System.Text;

namespace FiapProcessaVideo.Infrastructure.Messaging.Publishers
{
    public class NotificationPublisher : IMessagePublisher
    {
        private readonly IConnection _connection;
        private readonly IModel _channel;
        private const string Exchange = "notification";

        public NotificationPublisher()
        {
            var connectionFactory = new ConnectionFactory
            {
                HostName = "localhost"
            };

            _connection = connectionFactory.CreateConnection("notification-service-notification-publisher");
            _channel = _connection.CreateModel();
        }

        public void PublishNotificationCreated(NotificationCreatedEvent notificationEvent)
        {
            var payload = JsonConvert.SerializeObject(notificationEvent);
            var byteArray = Encoding.UTF8.GetBytes(payload);

            _channel.BasicPublish(Exchange, "notification-created", null, byteArray);
            Console.WriteLine("NotificationCreatedEvent Published");
        }
    }
}
