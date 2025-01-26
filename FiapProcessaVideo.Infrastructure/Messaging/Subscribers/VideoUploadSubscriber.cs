using FiapProcessaVideo.Infrastructure.Messaging.Model;
using FiapProcessaVideo.Infrastructure.Messaging.Model.Shared;
using FiapProcessaVideo.Infrastructure.Messaging.Mapping;
using FiapProcessaVideo.Domain;
using FiapProcessaVideo.Application.UseCases;
using Microsoft.Extensions.Options;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using Microsoft.Extensions.Hosting;
using Newtonsoft.Json;
using System.Text;
using Microsoft.Extensions.DependencyInjection;

namespace FiapProcessaVideo.Infrastructure.Messaging.Subscribers
{    
    public class VideoUploadeSubscriber : BackgroundService
    {
        private readonly IConnection _connection;
        private readonly IModel _channel;
        private readonly MessagingSubscriberSettings _messagingSettings;
        private readonly IServiceProvider _serviceProvider;

        public VideoUploadeSubscriber( IServiceProvider serviceProvider, IOptions<MessagingSubscriberSettings> messagingSettings)
        {
            _messagingSettings = messagingSettings.Value;
            _serviceProvider = serviceProvider;

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

                VideoMapping videoMapping = new VideoMapping();
                
                using (var scope = _serviceProvider.CreateScope())
                {
                    var processVideoUseCase = scope.ServiceProvider.GetRequiredService<IProcessVideoUseCase>();
                    // Use processVideoUseCase here
                    if (message != null)
                    {
                        Video videoDomain = videoMapping.ToDomain(message);
                        await processVideoUseCase.Execute(videoDomain);
                    } 
                    else 
                    {
                        throw new Exception($"The message received from RabbitMQ was null or empty.");
                    }
                }

                Console.WriteLine($"Message VideoUploadedEvent received with Email {message.Email}");

                _channel.BasicAck(eventArgs.DeliveryTag, false);
            };

            _channel.BasicConsume(_messagingSettings.QueueName, false, consumer);

            return Task.CompletedTask;
        }
    }
}
