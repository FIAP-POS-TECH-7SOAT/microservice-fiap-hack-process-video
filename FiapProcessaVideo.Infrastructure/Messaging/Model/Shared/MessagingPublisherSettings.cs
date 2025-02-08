namespace FiapProcessaVideo.Infrastructure.Messaging.Model.Shared
{
    public class MessagingPublisherSettings
    {
        public string ExchangeName { get; set; }
        public string QueueName { get; set; }
        public string Uri { get; set; }
    }
}
