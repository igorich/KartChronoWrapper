using KartChronoWrapper.Models;

namespace KartChronoWrapper.Services
{
    public interface IRemoteFilesService
    {
        Task SaveCurrentSession(List<PilotProfile> htmlContent);
        Task<IEnumerable<string>> GetList();
        Task<string> GetSession(string key);
    }
}
