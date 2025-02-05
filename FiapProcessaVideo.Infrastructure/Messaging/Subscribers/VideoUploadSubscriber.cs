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
                try
                {
                    var contentArray = eventArgs.Body.ToArray();
                    var contentString = Encoding.UTF8.GetString(contentArray);
                    var message = JsonConvert.DeserializeObject<PayloadVideoWrapper>(contentString);

                    if (message == null || message.Pattern != "file:uploaded")
                    {
                        _channel.BasicReject(eventArgs.DeliveryTag, true); // Reject non-matching messages, but keep on the queue.
                        return;
                    }

                    Console.WriteLine($"Message VideoUploadedEvent received with Email {message.Data.Email}");

                    using (var scope = _serviceProvider.CreateScope())
                    {
                        var processVideoUseCase = scope.ServiceProvider.GetRequiredService<ProcessVideoUseCase>();
                        var videoMapping = new VideoMapping();

                        Video videoDomain = videoMapping.ToDomain(message.Data);

                        // Acknowledge the message before processing
                        _channel.BasicAck(eventArgs.DeliveryTag, false);

                        await processVideoUseCase.Execute(videoDomain);
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Error processing message: {ex.Message}");
                    _channel.BasicReject(eventArgs.DeliveryTag, true); // Reject the message on failure, but keep on the queue.
                }
            };

            _channel.BasicConsume(_messagingSettings.QueueName, false, consumer);
            return Task.CompletedTask;
        }
    }
}
