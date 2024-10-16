namespace KartChronoWrapper.Services
{
    public class LocalFilesService
    {
        public async Task ListSessions()
        {
            var folder = GetCurrentStorageFolder();

        }
        private string GetCurrentStorageFolder()
        {
            var folder = Path.Combine(
                Directory.GetCurrentDirectory(),
                "storage",
                $"{DateTime.Today.ToShortDateString()}");
            if (!Directory.Exists(folder))
                Directory.CreateDirectory(folder);

            return folder;
        }
    }
}
