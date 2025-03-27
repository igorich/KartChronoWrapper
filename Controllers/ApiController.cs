using KartChronoWrapper.Services;
using Microsoft.AspNetCore.Mvc;
using Serilog;

namespace KartChronoWrapper.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class ApiController : Controller
    {
        private IRemoteFilesService _remoteFilesService;
        public ApiController()
        {
            _remoteFilesService = new S3FilesService();
        }

        [HttpGet("GetSessionsList")]
        public async Task<IActionResult> GetSessionsList([FromQuery]DateTime date)
        {
            Log.Debug($"{DateTime.Now}: GetSessionsList called");
            try
            {
                var list = await _remoteFilesService.GetList(date);
                var htmlContent = await new HtmlService().WrapToPage(list, date);
                Log.Debug($"{DateTime.Now}: GetSessionsList is fine");

                return Content(htmlContent, "text/html");
            }
            catch (Exception ex)
            {
                Log.Error($"Uncatched exception: {ex.Message}");
                throw;
            }
        }

        [HttpGet("GetSession")]
        public async Task<IActionResult> GetSession([FromQuery]string name, [FromQuery]string strDate)
        {
            var content = await _remoteFilesService.GetSession(name);

            return Content(content, "text/html");
        }

        [HttpPost("SaveSession")]
        public async Task<IActionResult> SaveSession()
        {
            string? trackId = Environment.GetEnvironmentVariable("TRACK_ID");

            var ws = new WsDataLoader();
            //var ws = new SeleniumPageSaver();
            await ws.SaveSession();

            return Ok();
        }
    }
}
