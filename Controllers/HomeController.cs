using KartChronoWrapper.Services;
using Microsoft.AspNetCore.Mvc;

namespace KartChronoWrapper.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class HomeController : Controller
    {
        private IRemoteFilesService _remoteFilesService;
        public HomeController()
        {
            _remoteFilesService = new S3FilesService();
        }

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
                Console.WriteLine("Warning. No track url set.");

            var htmlContent = await System.IO.File.ReadAllTextAsync(filePath);
            htmlContent = htmlContent.Replace("{{trackUrl}}", trackUrl);

            return Content(htmlContent, "text/html");
        }

        [HttpGet("GetSessionsList")]
        public async Task<IActionResult> GetSessionsList()
        {
            var list = await _remoteFilesService.GetList();
            var htmlContent = await new HtmlService().WrapToPage(list);

            return Content(htmlContent, "text/html");
        }

        [HttpGet("GetSession")]
        public async Task<IActionResult> GetSession([FromQuery]string name, [FromQuery]string strDate)
        {
            var date = DateTime.Parse(strDate);
            var filePath = Path.Combine($"storage\\{date.ToShortDateString()}\\{name}");
            if (!System.IO.File.Exists(filePath))
            {
                return NotFound("HTML файл не найден.");
            }
            var htmlContent = await System.IO.File.ReadAllTextAsync(filePath);

            return Content(htmlContent, "text/html");
        }

        [HttpPost("SaveSession")]
        public async Task<IActionResult> SaveSession()
        {
            string? trackId = Environment.GetEnvironmentVariable("TRACK_ID");

            var ws = trackId is null ? new WsDataLoader() : new WsDataLoader(trackId);// "1f3e81fc98c56b12aaeed4a1a4eb91cb");
            ws.LoadData();

            return Ok();

        }
    }
}
