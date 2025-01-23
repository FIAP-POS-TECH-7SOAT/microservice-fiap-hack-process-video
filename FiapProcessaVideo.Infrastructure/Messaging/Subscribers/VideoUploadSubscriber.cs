using FiapProcessaVideo.Infrastructure.Messaging.Model;
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
        private const string Queue = "video-uploaded";

        public VideoUploadeSubscriber()
        {
            var connectionFactory = new ConnectionFactory
            {
                HostName = "localhost"
            };

            _connection = connectionFactory.CreateConnection("video-service-video-uploaded-consumer");

            _channel = _connection.CreateModel();
        }

        protected override Task ExecuteAsync(CancellationToken stoppingToken)
        {
            var consumer = new EventingBasicConsumer(_channel);

            consumer.Received += async (sender, eventArgs) =>
            {
                var contentArray = eventArgs.Body.ToArray();
                var contentString = Encoding.UTF8.GetString(contentArray);
                var message = JsonConvert.DeserializeObject<VideoResponse>(contentString);

                Console.WriteLine($"Message VideoResponse received with Email {message.Email}");

                _channel.BasicAck(eventArgs.DeliveryTag, false);
            };

            _channel.BasicConsume(Queue, false, consumer);

            return Task.CompletedTask;
        }
    }
}
