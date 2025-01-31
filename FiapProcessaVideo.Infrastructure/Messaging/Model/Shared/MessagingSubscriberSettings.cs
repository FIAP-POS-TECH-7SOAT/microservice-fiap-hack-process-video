namespace FiapProcessaVideo.Infrastructure.Messaging.Model.Shared
{
    public class MessagingSubscriberSettings
    {
        public string HostName { get; set; }
        public string QueueName { get; set; }
        public string Password { get; set; }
        public string UserName { get; set; }
        public int Port { get; set; }
        public string VirtualHost { get; set; }
    }
}
