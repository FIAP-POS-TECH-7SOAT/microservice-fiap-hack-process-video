using FiapProcessaVideo.Application.UseCases;
using FiapProcessaVideo.Domain;
using Microsoft.AspNetCore.Mvc;

namespace FiapProcessaVideo.WebApi.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class VideoProcessController : ControllerBase
    {
        private readonly IProcessVideoUseCase _processVideoUseCase;

        public VideoProcessController(IProcessVideoUseCase processVideoUseCase)
        {
            _processVideoUseCase = processVideoUseCase;
        }

        [HttpPost("process")]
        public async Task<IActionResult> ProcessVideo([FromBody] string videoKey)
        {
            Video video = Video.Load(@"", videoKey, new TimeSpan(240), new TimeSpan(10));
            var result = await _processVideoUseCase.Execute(video);
            return Ok(new { ZipFilePath = result });
        }
    }
}