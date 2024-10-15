using System.Net;

namespace KartChronoWrapper.Services
{
    public interface IRemoteFilesService
    {
        Task SaveCurrentRace();
        Task <IEnumerable<string>> GetList();
    }

    public class RemoteFilesService : IRemoteFilesService
    {

        public async Task SaveCurrentRace()
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
                content = oVerwriteStylesLinks(content);

                await File.WriteAllTextAsync(
                    Path.Combine(GetCurrentStorageFolder(), $"Заезд-{new Random().Next(1, 10)}.html"),
                    content);
            }

        }

        private string oVerwriteStylesLinks(string content)
        {
            return content.Replace("href='/css/", "href='https://stkmotor.kartchrono.com/css/");
        }

        public async Task<IEnumerable<string>> GetList()
        {
            var dir = new DirectoryInfo(GetCurrentStorageFolder());

            return dir.EnumerateFiles().Select(i => i.Name);
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
