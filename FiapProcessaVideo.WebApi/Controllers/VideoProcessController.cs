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
        private readonly IProcessVideoUseCase _processVideoUseCase;
        private readonly NotificationPublisher _notificationPublisher;

        public VideoProcessController(IProcessVideoUseCase processVideoUseCase, NotificationPublisher notificationPublisher)
        {
            _processVideoUseCase = processVideoUseCase;
            _notificationPublisher = notificationPublisher;
        }

        [HttpPost("process")]
        public async Task<IActionResult> ProcessVideo([FromBody] string videoKey)
        {
            Video video = Video.Load(@"", videoKey, new TimeSpan(240), new TimeSpan(10));
            var result = await _processVideoUseCase.Execute(video);

            NotificationCreatedEvent notificationCreatedEvent = new NotificationCreatedEvent();
            notificationCreatedEvent.Id = Guid.NewGuid().ToString();
            notificationCreatedEvent.Email = "rafa.yuji@gmail.com";
            notificationCreatedEvent.File = videoKey;
            notificationCreatedEvent.Status = "processed";
            notificationCreatedEvent.UserId = "";
            _notificationPublisher.PublishNotificationCreated(notificationCreatedEvent);
            return Ok(new { ZipFilePath = result });
        }
    }
}