using FiapProcessaVideo.Infrastructure.Messaging.Model;
using FiapProcessaVideo.Infrastructure.Messaging.Model.Shared;
using Microsoft.Extensions.Options;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using Microsoft.Extensions.Hosting;
using Newtonsoft.Json;
using System.Text;

namespace FiapProcessaVideo.Infrastructure.Messaging.Subscribers
{    
    public class VideoUploadeSubscriber : BackgroundService
    {
        private readonly IConnection _connection;
        private readonly IModel _channel;
        private readonly MessagingSubscriberSettings _messagingSettings;

        public VideoUploadeSubscriber(IOptions<MessagingSubscriberSettings> messagingSettings)
        {
            _messagingSettings = messagingSettings.Value;

            var connectionFactory = new ConnectionFactory
            {
                HostName = _messagingSettings.HostName
            };

            _connection = connectionFactory.CreateConnection("VideoUploadSubscriberConnection");

            _channel = _connection.CreateModel();
        }

        protected override Task ExecuteAsync(CancellationToken stoppingToken)
        {
            var consumer = new EventingBasicConsumer(_channel);

            consumer.Received += async (sender, eventArgs) =>
            {
                var contentArray = eventArgs.Body.ToArray();
                var contentString = Encoding.UTF8.GetString(contentArray);
                var message = JsonConvert.DeserializeObject<VideoUploadedEvent>(contentString);

                Console.WriteLine($"Message VideoUploadedEvent received with Email {message.Email}");

                _channel.BasicAck(eventArgs.DeliveryTag, false);
            };

            _channel.BasicConsume(_messagingSettings.QueueName, false, consumer);

            return Task.CompletedTask;
        }
    }
}
