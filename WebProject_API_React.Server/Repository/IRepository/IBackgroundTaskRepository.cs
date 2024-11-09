using WebProject_API_React.Server.Models;

namespace WebProject_API_React.Server.Repository.IRepository
{
    public interface IBackgroundTaskRepository : IRepository<BackgroundTask>
    {
        Task SaveChangesAsync(CancellationToken cancellationToken = default);
    }
}
