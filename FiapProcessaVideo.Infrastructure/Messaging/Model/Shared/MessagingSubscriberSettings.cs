namespace FiapProcessaVideo.Infrastructure.Messaging.Model.Shared
{
    public class MessagingSubscriberSettings
    {
        public string QueueName { get; set; }
        public string VirtualHost { get; set; }
        public string Uri { get; set; }
    }
}
