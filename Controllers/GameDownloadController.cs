using Microsoft.AspNetCore.Mvc;

namespace YourNamespace.Controllers
{
    [Route("api/gameDownload")]
    [ApiController]
    public class GameDownloadController : ControllerBase
    {
        // GET api/download/frontend
        [HttpGet("download")]
        public IActionResult GetFrontendApp()
        {
            var filePath = Path.Combine(Directory.GetCurrentDirectory(), "Downloads", "MyApp.rar");
            if (!System.IO.File.Exists(filePath))
            {
                return NotFound();
            }
            var mimeType = "application/rar"; 
            var fileName = "MyApp.rar";
            return PhysicalFile(filePath, mimeType, fileName);
        }
    }
}