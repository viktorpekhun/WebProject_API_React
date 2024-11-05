using WebProject_API_React.Server.Data;
using WebProject_API_React.Server.Models;
using WebProject_API_React.Server.Repository.IRepository;

namespace WebProject_API_React.Server.Repository
{
    public class BackgroundTaskRepository : Repository<BackgroundTask>, IBackgroundTaskRepository
    {
        private ApplicationDbContext _context;

        public BackgroundTaskRepository(ApplicationDbContext context) : base(context)
        {
            _context = context;
        }

        public async Task SaveChangesAsync(CancellationToken cancellationToken = default)
        {
            await _context.SaveChangesAsync(cancellationToken);
        }
    }
}
