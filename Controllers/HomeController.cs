using KartChronoWrapper.Services;
using Microsoft.AspNetCore.Mvc;

namespace KartChronoWrapper.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class HomeController : Controller //Base
    {
        private IRemoteFilesService _remoteFilesService;
        public HomeController()
        {
            _remoteFilesService = new RemoteFilesService();
        }

        [HttpGet("Index")]
        public async Task<IActionResult> Index()
        {
            /*Stream stream = new FileStream("index.html", FileMode.Open);
            if (stream == null)
                return NotFound();
            return File(stream, "text/html", "index.html");*/
            //return File("index.html", "text/html");
            var filePath = Path.Combine("index.html");
            if (!System.IO.File.Exists(filePath))
            {
                return NotFound("HTML файл не найден.");
            }
            var htmlContent = await System.IO.File.ReadAllTextAsync(filePath);
            return Content(htmlContent, "text/html");
        }

        [HttpPost("SaveRace")]
        public IActionResult SaveRace()
        {
            _remoteFilesService.SaveCurrentRace();
            return this.Ok();
        }

        [HttpGet("GetRacesList")]
        public IActionResult GetRacesList()
        {
            var list = _remoteFilesService.GetList();
            return this.Ok(list);
        }

        [HttpGet("GetRace/")]
        public IActionResult GetRace()
        {
            return this.Ok();
        }
    }
}
