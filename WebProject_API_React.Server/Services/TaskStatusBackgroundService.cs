using System.Collections.Concurrent;
using WebProject_API_React.Server.Models;
using WebProject_API_React.Server.Repository.IRepository;

namespace WebProject_API_React.Server.Services
{
    public class TaskStatusBackgroundService : BackgroundService
    {
        private readonly IServiceScopeFactory _scopeFactory;
        private readonly ConcurrentDictionary<int, (string Status, DateTime ExpirationTime)> _taskStatusDictionary;
        private readonly TimeSpan _ttl = TimeSpan.FromSeconds(6);

        public TaskStatusBackgroundService(IServiceScopeFactory scopeFactory)
        {
            _scopeFactory = scopeFactory;
            _taskStatusDictionary = new ConcurrentDictionary<int, (string, DateTime)>();
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            while (!stoppingToken.IsCancellationRequested)
            {
                using (var scope = _scopeFactory.CreateScope())
                {
                    var backgroundTaskRepository = scope.ServiceProvider.GetRequiredService<IBackgroundTaskRepository>();

                    var cancelledTasks = await backgroundTaskRepository.GetListAsync(
                t => t.Status == "Cancelled", stoppingToken);

                    var newCancelledTasks = cancelledTasks
                .Where(task => !_taskStatusDictionary.ContainsKey(task.Id));

                    var currentTime = DateTime.UtcNow;
                    foreach (var task in newCancelledTasks)
                    {
                        _taskStatusDictionary[task.Id] = ("Cancelled", currentTime + _ttl);
                    }

                    foreach (var key in _taskStatusDictionary.Keys)
                    {
                        if (_taskStatusDictionary.TryGetValue(key, out var entry) && entry.ExpirationTime <= currentTime)
                        {
                            _taskStatusDictionary.TryRemove(key, out _);
                        }
                    }
                }

                await Task.Delay(TimeSpan.FromSeconds(2), stoppingToken);
            }
        }

        public async Task RestartTaskAsync(BackgroundTask task)
        {
            using (var scope = _scopeFactory.CreateScope())
            {
                var backgroundTaskRepository = scope.ServiceProvider.GetRequiredService<IBackgroundTaskRepository>();

                if (task != null)
                {
                    task.Status = "Pending";

                    await backgroundTaskRepository.UpdateAsync(task);

                    _taskStatusDictionary.TryRemove(task.Id, out _);


                }
            }
        }

        public bool IsTaskCancelled(int taskId)
        {
            if (_taskStatusDictionary.TryGetValue(taskId, out var entry) && entry.Status == "Cancelled")
            {
                if (entry.ExpirationTime > DateTime.UtcNow)
                {
                    return true;
                }
                else
                {
                    _taskStatusDictionary.TryRemove(taskId, out _);
                }
            }
            return false;
        }
    }
}
