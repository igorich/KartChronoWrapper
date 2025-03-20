using KartChronoWrapper.Models;

namespace KartChronoWrapper.Services
{
    public interface IRemoteFilesService
    {
        Task<IEnumerable<string>> GetList(DateTime date);
        Task<string> GetSession(string key);
        Task SaveCurrentSession(List<PilotProfile> htmlContent);
    }
}
