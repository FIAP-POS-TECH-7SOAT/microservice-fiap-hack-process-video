using FiapProcessaVideo.Infrastructure.Messaging.Model.Shared;
using FiapProcessaVideo.Application.Mapping;
using FiapProcessaVideo.Application.UseCases;
using FiapProcessaVideo.Application.Model;
using FiapProcessaVideo.Domain;
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

        public VideoUploadeSubscriber(IServiceProvider serviceProvider, IOptions<MessagingSubscriberSettings> messagingSettings)
        {
            _messagingSettings = messagingSettings.Value;
            _serviceProvider = serviceProvider;

            // var connectionFactory = new ConnectionFactory
            // {
            //     HostName = _messagingSettings.HostName,
            //     Password = _messagingSettings.Password,
            //     Port = _messagingSettings.Port,
            //     UserName = _messagingSettings.UserName,
            //     VirtualHost = _messagingSettings.VirtualHost,
            //     Uri = 

            // };
            ConnectionFactory connectionFactory = new ConnectionFactory();
            connectionFactory.Uri = new Uri(_messagingSettings.Uri);
            Console.WriteLine(_messagingSettings.Uri);
            _connection = connectionFactory.CreateConnection("microservice-fiap-processa-video-upload-subscriber-connection");

            _channel = _connection.CreateModel();
        }

        protected override Task ExecuteAsync(CancellationToken stoppingToken)
        {
            var consumer = new EventingBasicConsumer(_channel);

            consumer.Received += async (sender, eventArgs) =>
            {
                var contentArray = eventArgs.Body.ToArray();
                var contentString = Encoding.UTF8.GetString(contentArray);
                var message = JsonConvert.DeserializeObject<PayloadVideoWrapper>(contentString);

                VideoMapping videoMapping = new VideoMapping();
                
                if (message != null)
                {
                    if (message.Pattern == "file:uploaded")
                    {
                        Console.WriteLine($"Message VideoUploadedEvent received with Email {message.Data.Email}");
                        using (var scope = _serviceProvider.CreateScope())
                        {
                            var processVideoUseCase = scope.ServiceProvider.GetRequiredService<ProcessVideoUseCase>();
                            // Use processVideoUseCase here
                            if (message != null)
                            {
                                _channel.BasicAck(eventArgs.DeliveryTag, false);
                                Video videoDomain = videoMapping.ToDomain(message.Data);
                                await processVideoUseCase.Execute(videoDomain);
                            } 
                            else 
                            {
                                throw new Exception($"The message received from RabbitMQ was null or empty.");
                            }
                        }
                    }
                }
            };

            _channel.BasicConsume(_messagingSettings.QueueName, false, consumer);

            return Task.CompletedTask;
        }
    }
}
