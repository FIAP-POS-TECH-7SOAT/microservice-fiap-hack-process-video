//using FiapLanchonete.Application.UseCases.Interface;
//using FiapLanchonete.Domain;
//using FiapLanchonete.Domain.Orders;
//using FiapLanchonete.Domain.ValueObjects;
//using FiapLanchonete.WebApi.Model;
//using FiapLanchonete.WebApi.Utils;
using FiapProcessaVideo.Application.UseCases;
using FiapProcessaVideo.Domain;
using FiapProcessaVideo.WebApi.Model;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using System.Text;
using System.Text.Json;

namespace FiapProcessaVideo.WebApi.Services
{
    public class RabbitMqConsumerService : BackgroundService
    {
        private readonly IConnection _connection;
        private readonly IProcessVideoUseCase _processVideoUseCase;
        private IModel _channel;

        public RabbitMqConsumerService(IConnection connection, IModel model, IProcessVideoUseCase processVideoUseCase)
        {
            _connection = connection;
            _channel = model; // Create a channel from the shared connection
            _processVideoUseCase = processVideoUseCase;
        }

        protected override Task ExecuteAsync(CancellationToken stoppingToken)
        {
            var consumer = new EventingBasicConsumer(_channel);
            consumer.Received += async (model, ea) =>
            {
                var body = ea.Body.ToArray();
                var message = Encoding.UTF8.GetString(body);
                try
                {
                    var options = new JsonSerializerOptions
                    {
                        PropertyNameCaseInsensitive = true
                    };
                    var videoResponse = JsonSerializer.Deserialize<VideoResponseWrapper>(message, options);

                    if (videoResponse != null)
                    {
                        var video = Video.Load(videoResponse.Data.VideoFilePath, videoResponse.Data.Duration, videoResponse.Data.SnapshotInterval);
                        var zipfilepath = _processVideoUseCase.Execute(video);
                    }

                    // Acknowledge the message
                    // Acknowledge the message after successful processing
                    _channel.BasicAck(deliveryTag: ea.DeliveryTag, multiple: false);
                }
                catch (Exception ex)
                {
                    // Acknowledge the message after error, not using BasicNack.
                    _channel.BasicAck(deliveryTag: ea.DeliveryTag, multiple: false);
                }
            };

            _channel.BasicConsume(queue: _channel.CurrentQueue, autoAck: false, consumer: consumer);

            return Task.CompletedTask;
        }

        public override void Dispose()
        {
            _channel?.Close();
            _channel?.Dispose();
            base.Dispose();
        }
    }
}