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
            _remoteFilesService = new RemoteFilesService();
        }

        [HttpGet("Index")]
        public async Task<IActionResult> Index()
        {
            var filePath = Path.Combine("index.html");
            if (!System.IO.File.Exists(filePath))
            {
                return NotFound("HTML файл не найден.");
            }
            var htmlContent = await System.IO.File.ReadAllTextAsync(filePath);

            return Content(htmlContent, "text/html");
        }

        [HttpPost("SaveSession")]
        public IActionResult SaveSession()
        {
            _remoteFilesService.SaveCurrentSession();
            return this.Ok();
        }

        [HttpGet("GetSessionsList")]
        public async Task<IActionResult> GetSessionsList()
        {
            var list = await _remoteFilesService.GetList();
            var htmlContent = _remoteFilesService.WrapToPage(list);

            return Content(htmlContent, "text/html");
        }

        [HttpGet("GetSession")]
        public async Task<IActionResult> GetSession([FromQuery]string name, string strDate)
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
    }
}
