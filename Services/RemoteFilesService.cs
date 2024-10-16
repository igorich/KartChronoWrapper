using System.Net;
using System.Text;

namespace KartChronoWrapper.Services
{
    public interface IRemoteFilesService
    {
        Task SaveCurrentSession();
        Task <IEnumerable<string>> GetList();
        string WrapToPage(IEnumerable<string> sessions);
    }

    public class RemoteFilesService : IRemoteFilesService
    {
        public async Task SaveCurrentSession()
        {
            //
            var url = "https://stkmotor.kartchrono.com/online/?variant=monitor";
            using (var client = new HttpClient())
            {
                client.DefaultRequestHeaders.Accept.Clear();
                var response = await client.GetAsync(url).ConfigureAwait(false);
                if (response.StatusCode != HttpStatusCode.OK)
                {
                    Console.WriteLine(response.StatusCode.ToString());
                    return;
                }
                if (response == null)
                    return;

                var content = await response.Content.ReadAsStringAsync().ConfigureAwait(false);
                content = OverwriteStylesLinks(content);

                await File.WriteAllTextAsync(
                    Path.Combine(GetCurrentStorageFolder(), $"Заезд-{new Random().Next(1, 10)}.html"),
                    content);
            }
        }

        private string OverwriteStylesLinks(string content)
        {
            return content.Replace("href='/css/", "href='https://stkmotor.kartchrono.com/css/");
        }

        public async Task<IEnumerable<string>> GetList()
        {
            var dir = new DirectoryInfo(GetCurrentStorageFolder());

            return dir.EnumerateFiles().Select(i => i.Name);
        }

        public string WrapToPage(IEnumerable<string> sessions)
        {
            var header = "<!DOCTYPE HTML>\r\n<html>\r\n<head>\r\n    <meta charset=\"utf-8\">\r\n<link rel='stylesheet' href='https://stkmotor.kartchrono.com/css/colors.css?v=1690359523'/>\r\n<link rel='stylesheet' href='https://stkmotor.kartchrono.com/css/scoreboard2.css?v=1727079568'/>\r\n<link rel='stylesheet' href='https://stkmotor.kartchrono.com/css/osd-stream.css?v=1694183475'/>\r\n<link rel='stylesheet' href='https://stkmotor.kartchrono.com/css/nav.css?v=1632461511'/>\r\n<link rel='stylesheet' href='https://stkmotor.kartchrono.com/css/records.css?v=1670413288'/>\r\n<link rel='stylesheet' href='https://stkmotor.kartchrono.com/css/archive.css?v=1632316200'/>\r\n</head>\r\n<body>\r\n";
            var footer= "</body>\r\n</html>";
            var body = new StringBuilder();
            foreach (var i in sessions)
            {
                body.Append($"<p><a href=\"getSession?date={DateTime.Today.ToShortDateString()}&name={i}\" >{i}</a></p>");
            }
            return header + body.ToString() + footer;
        }

        private string GetCurrentStorageFolder()
        {
            var folder = Path.Combine(
                Directory.GetCurrentDirectory(),
                "storage",
                $"{DateTime.Today.ToShortDateString()}");
            if(!Directory.Exists(folder))
                Directory.CreateDirectory(folder);

            return folder;
        }
    }
}
