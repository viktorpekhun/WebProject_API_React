using System.Collections.Concurrent;
using WebProject_API_React.Server.Models;
using WebProject_API_React.Server.Repository.IRepository;

namespace WebProject_API_React.Server.Services
{
    public class TaskStatusBackgroundService : BackgroundService
    {
        private readonly IServiceScopeFactory _scopeFactory;
        private readonly ConcurrentDictionary<int, (string Status, DateTime ExpirationTime)> _taskStatusDictionary;
        private readonly TimeSpan _ttl = TimeSpan.FromSeconds(6); // TTL duration, e.g., 5 minutes

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

                    // Fetch tasks with "Cancelled" status from the database
                    var cancelledTasks = await backgroundTaskRepository.GetListAsync(
                t => t.Status == "Cancelled", stoppingToken);

                    var newCancelledTasks = cancelledTasks
                .Where(task => !_taskStatusDictionary.ContainsKey(task.Id));

                    // Update the in-memory dictionary with the latest statuses and expiration times
                    var currentTime = DateTime.UtcNow;
                    foreach (var task in newCancelledTasks)
                    {
                        _taskStatusDictionary[task.Id] = ("Cancelled", currentTime + _ttl);
                    }

                    // Видаляємо прострочені записи зі словника
                    foreach (var key in _taskStatusDictionary.Keys)
                    {
                        if (_taskStatusDictionary.TryGetValue(key, out var entry) && entry.ExpirationTime <= currentTime)
                        {
                            _taskStatusDictionary.TryRemove(key, out _);
                        }
                    }
                }

                // Wait for the next cycle (e.g., 2 seconds) before checking again
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
                    task.Status = "Pending"; // Change status to Pending

                    // Save the updated task in the database
                    await backgroundTaskRepository.UpdateAsync(task);

                    // 2. Update dictionary: Remove cancelled entry and add new "Pending" entry
                    _taskStatusDictionary.TryRemove(task.Id, out _); // Remove cancelled task from the dictionary


                }
            }
        }

        public bool IsTaskCancelled(int taskId)
        {
            // Check if the task is marked as "Cancelled" and its TTL has not expired
            if (_taskStatusDictionary.TryGetValue(taskId, out var entry) && entry.Status == "Cancelled")
            {
                // Ensure the TTL has not expired
                if (entry.ExpirationTime > DateTime.UtcNow)
                {
                    return true;
                }
                else
                {
                    // Remove expired entry if TTL has passed
                    _taskStatusDictionary.TryRemove(taskId, out _);
                }
            }
            return false;
        }
    }
}
