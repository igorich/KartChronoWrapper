using Microsoft.AspNetCore.Mvc;
using Serilog;

namespace KartChronoWrapper.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class HomeController : Controller
    {
        [HttpGet("Index")]
        public async Task<IActionResult> Index()
        {
            var filePath = Path.Combine("WebPages/index.html");
            if (!System.IO.File.Exists(filePath))
            {
                return NotFound("HTML файл не найден.");
            }
            string? trackUrl = Environment.GetEnvironmentVariable("TRACK_URL");
            if (trackUrl is null)
                Log.Warning("Warning. No track url set.");

            var htmlContent = await System.IO.File.ReadAllTextAsync(filePath);
            htmlContent = htmlContent.Replace("{{trackUrl}}", trackUrl);

            return Content(htmlContent, "text/html");
        }
    }
}
