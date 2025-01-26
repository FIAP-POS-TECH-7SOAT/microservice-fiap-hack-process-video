namespace FiapProcessaVideo.Infrastructure.Messaging.Model.Shared
{
    public class MessagingPublisherSettings
    {
        public string HostName { get; set; }
        public string ExchangeName { get; set; }
        public string RoutingKey { get; set; }
    }
}
