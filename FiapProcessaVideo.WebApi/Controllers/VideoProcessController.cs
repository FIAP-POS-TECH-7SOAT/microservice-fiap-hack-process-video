using FiapProcessaVideo.Application.UseCases;
using FiapProcessaVideo.Domain;
using FiapProcessaVideo.Infrastructure.Messaging.Publishers;
using FiapProcessaVideo.Infrastructure.Messaging.Model;
using Microsoft.AspNetCore.Mvc;

namespace FiapProcessaVideo.WebApi.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class VideoProcessController : ControllerBase
    {
        private readonly NotificationPublisher _notificationPublisher;

        public VideoProcessController(NotificationPublisher notificationPublisher)
        {
            _notificationPublisher = notificationPublisher;
        }
    }
}