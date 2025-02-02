using FiapProcessaVideo.Application.Messaging.Interfaces;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using Microsoft.Extensions.Hosting;
using Newtonsoft.Json;
using System.Text;
using Microsoft.Extensions.Options;
using FiapProcessaVideo.Infrastructure.Messaging.Model.Shared;
using FiapProcessaVideo.Application.Model;

namespace FiapProcessaVideo.Infrastructure.Messaging.Publishers
{
    public class NotificationPublisher : IMessagePublisher
    {
        private readonly IConnection _connection;
        private readonly IModel _channel;
        private readonly string _exchange;
        private readonly List<string> _routingKeys;

        private readonly MessagingPublisherSettings _messagingSettings;

        public NotificationPublisher(IOptions<MessagingPublisherSettings> messagingSettings)
        {
            _messagingSettings = messagingSettings.Value;

            ConnectionFactory connectionFactory = new ConnectionFactory();
            connectionFactory.Uri = new Uri(_messagingSettings.Uri);

            _exchange = _messagingSettings.ExchangeName;
            _routingKeys = _messagingSettings.RoutingKeys;

            _connection = connectionFactory.CreateConnection("microservice-fiap-processa-video-notification-publisher-connection");
            _channel = _connection.CreateModel();
        }

        public void PublishNotificationCreated(PayloadVideoWrapper payloadVideo, string status)
        {
            var payload = JsonConvert.SerializeObject(payloadVideo);
            var byteArray = Encoding.UTF8.GetBytes(payload);
            string routingKey;

            switch (status)
            {
                case "processing":
                    routingKey = _routingKeys[0].ToString();
                    break;
                case "processed":
                    routingKey= _routingKeys[1].ToString();
                    break;
                default:
                    throw new Exception($"Invalid status: {status}.");
            }

            _channel.BasicPublish(_exchange, routingKey, null, byteArray);
            Console.WriteLine("NotificationCreatedEvent Published");
        }
    }
}
