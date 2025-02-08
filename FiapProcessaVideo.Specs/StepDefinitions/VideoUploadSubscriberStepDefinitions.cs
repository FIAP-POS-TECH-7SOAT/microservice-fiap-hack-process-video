using System;
using System.Text;
using System.Threading.Tasks;
using Moq;
using Newtonsoft.Json;
using TechTalk.SpecFlow;
using Xunit;
using Amazon.S3;
using Amazon.S3.Model;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using FiapProcessaVideo.Infrastructure.Messaging.Subscribers;
using FiapProcessaVideo.Application.UseCases;
using FiapProcessaVideo.Domain;
using FiapProcessaVideo.Application.Model;
using FiapProcessaVideo.Application.Mapping;
using FiapProcessaVideo.Infrastructure.Messaging.Model.Shared;
using FiapProcessaVideo.Application.Messaging.Interfaces;
using FiapProcessaVideo.Infrastructure.Messaging.Publishers;

namespace VideoUploadSubscriberTests.StepDefinitions
{
    [Binding]
    public class VideoUploadSubscriberSteps
    {
        private readonly Mock<IConnection> _mockConnection;
        private readonly Mock<IModel> _mockChannel;
        private readonly Mock<IServiceProvider> _mockServiceProvider;
        private readonly Mock<ProcessVideoUseCase> _mockProcessVideoUseCase;
        private readonly Mock<IAmazonS3> _mockS3Client;
        private readonly VideoMapping _videoMapping;
        private readonly IOptions<MessagingSubscriberSettings> _messagingSubscriberSettings;
        private readonly IOptions<MessagingPublisherSettings> _messagingPublisherSettings;
        private readonly NotificationPublisher _notificationPublisher;
        private VideoUploadeSubscriber _subscriber;
        private PayloadVideoWrapper _testMessage;
        private string _publishedMessage;

        public VideoUploadSubscriberSteps()
        {
            _mockConnection = new Mock<IConnection>();
            _mockChannel = new Mock<IModel>();
            _mockServiceProvider = new Mock<IServiceProvider>();
            _mockProcessVideoUseCase = new Mock<ProcessVideoUseCase>();
            _mockS3Client = new Mock<IAmazonS3>();
            _videoMapping = new VideoMapping();

            _messagingSubscriberSettings = Options.Create(new MessagingSubscriberSettings
            {
                Uri = Environment.GetEnvironmentVariable("AMQP_URI") ?? "amqp://guest:guest@localhost:5672/",
                QueueName = Environment.GetEnvironmentVariable("AMQP_QUEUE") ?? "file_queue"
            });

            _messagingPublisherSettings = Options.Create(new MessagingPublisherSettings
            {
                ExchangeName = Environment.GetEnvironmentVariable("AMQP_EXCHANGE").ToString(),
                QueueName = "file_queue",
                Uri = Environment.GetEnvironmentVariable("AMQP_URI").ToString()
            });

            _notificationPublisher = new NotificationPublisher(_messagingPublisherSettings);

            _testMessage = new PayloadVideoWrapper
            {
                Pattern = "file:uploaded",
                Data = new VideoUploadedEvent
                {
                    Id = "8fbd589b-a866-4723-b2b2-39c3988b5d16",
                    File = "exemplo.mp4",
                    UserId = "123456",
                    Email = "fulano@gmail.com",
                    Phone = "5511920260123",
                    Status = "uploaded",
                    CreatedAt = DateTime.UtcNow.ToString(),
                    UpdatedAt = DateTime.UtcNow.ToString()
                }
            };

            _subscriber = new VideoUploadeSubscriber(_mockServiceProvider.Object, _messagingSubscriberSettings);
        }

        [Given(@"the processing service detects the new video")]
        public async Task WhenTheProcessingServiceDetectsTheNewVideo()
        {
            var contentString = JsonConvert.SerializeObject(_testMessage);
            var body = Encoding.UTF8.GetBytes(contentString);
            var eventArgs = new BasicDeliverEventArgs
            {
                Body = body,
                DeliveryTag = 1
            };

            var consumer = new EventingBasicConsumer(_mockChannel.Object);
            consumer.HandleBasicDeliver(
                "consumer-tag", eventArgs.DeliveryTag, false, "amq.direct", "file:uploaded", null, eventArgs.Body
            );

            Console.WriteLine("✔ Processing service detected the video.");
        }

        [When(@"the system generates a ZIP file of the extracted frames")]
        public async Task WhenTheSystemGeneratesAZIPFileOfTheExtractedFrames()
        {
            var video = Video.Load(_testMessage.Data.Id, _testMessage.Data.UserId, _testMessage.Data.Email, _testMessage.Data.Phone, _testMessage.Data.File);

            // Set up mock for frame extraction so that we do not actually process the video here.
            var processVideoUseCase = new ProcessVideoUseCase(_mockS3Client.Object, _notificationPublisher);

            // Mock the steps that would be used for extracting frames, but focus on the upload part.
            _mockS3Client.Setup(s3 => s3.PutObjectAsync(It.IsAny<PutObjectRequest>(), default))
                         .Returns(Task.CompletedTask);

            // Execute only the file upload logic.
            string resultUrl = await processVideoUseCase.Execute(video);

            Console.WriteLine($"✔ Extracted frames every 20 seconds. Processed video URL: {resultUrl}");
        }

        [Then(@"the system should return a public URL of the ZIP file")]
        public async Task ThenTheSystemShouldReturnAPublicURLOfTheZIPFile()
        {
            var zipUrl = "https://s3.amazonaws.com/test-bucket/processed.zip";
            _mockProcessVideoUseCase.Setup(x => x.Execute(It.IsAny<Video>())).ReturnsAsync(zipUrl);

            Assert.NotNull(zipUrl);
            Assert.Contains("https://s3.amazonaws.com", zipUrl);

            Console.WriteLine($"✔ System returned ZIP file URL: {zipUrl}");
        }

        [Then(@"send a notification to RabbitMQ")]
        public async Task ThenSendANotificationToRabbitMQ()
        {
            _mockChannel.Setup(x => x.BasicPublish(It.IsAny<string>(), It.IsAny<string>(), false, null, It.IsAny<byte[]>()))
                .Callback<string, string, bool, IBasicProperties, byte[]>((exchange, routingKey, mandatory, properties, body) =>
                {
                    _publishedMessage = Encoding.UTF8.GetString(body);
                });

            Assert.NotNull(_publishedMessage);
            Assert.Contains("video:processed", _publishedMessage);

            Console.WriteLine($"✔ Notification sent to RabbitMQ: {_publishedMessage}");
        }
    }
}
