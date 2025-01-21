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
        public async Task<IActionResult> ProcessVideo()
        {
            //Video video = Video.Load("/home/yuji/Documentos/git-repos/Microservice-Fiap-Processa-Video/FiapProcessaVideo.WebApi/",
            Video video = Video.Load(@"", "Marvel_DOTNET_CSHARP.mp4", new TimeSpan(240), new TimeSpan(10));
            var result = await _processVideoUseCase.Execute(video);
            return Ok(new { ZipFilePath = result });
        }
    }
}