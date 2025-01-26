using FiapProcessaVideo.Infrastructure.Messaging.Publishers.Interfaces;
using FiapProcessaVideo.Infrastructure.Messaging.Model;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using Microsoft.Extensions.Hosting;
using Newtonsoft.Json;
using System.Text;
using Microsoft.Extensions.Options;
using FiapProcessaVideo.Infrastructure.Messaging.Model.Shared;

namespace FiapProcessaVideo.Infrastructure.Messaging.Publishers
{
    public class NotificationPublisher : IMessagePublisher
    {
        private readonly IConnection _connection;
        private readonly IModel _channel;
        private readonly string _exchange;
        private readonly string _routingKey;

        private readonly MessagingPublisherSettings _messagingSettings;

        public NotificationPublisher(IOptions<MessagingPublisherSettings> messagingSettings)
        {
            _messagingSettings = messagingSettings.Value;

            var connectionFactory = new ConnectionFactory
            {
                HostName = _messagingSettings.HostName
            };

            _exchange = _messagingSettings.ExchangeName;
            _routingKey = _messagingSettings.RoutingKey;

            _connection = connectionFactory.CreateConnection("notification-service-notification-publisher");
            _channel = _connection.CreateModel();
        }

        public void PublishNotificationCreated(NotificationCreatedEvent notificationEvent)
        {
            var payload = JsonConvert.SerializeObject(notificationEvent);
            var byteArray = Encoding.UTF8.GetBytes(payload);

            _channel.BasicPublish(_exchange, _routingKey, null, byteArray);
            Console.WriteLine("NotificationCreatedEvent Published");
        }
    }
}
