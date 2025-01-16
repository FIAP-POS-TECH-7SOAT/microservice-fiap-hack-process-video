using Microsoft.AspNetCore.Mvc;

namespace FiapProcessaVideo.WebApi.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class VideoProcessController : ControllerBase
    {
        [HttpGet]
        public IActionResult Index()
        {
            return Ok();
        }
    }
}
