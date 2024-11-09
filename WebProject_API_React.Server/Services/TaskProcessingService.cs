using WebProject_API_React.Server.Repository.IRepository;
using WebProject_API_React.Server.Models;
using System.Text.Json;
using System.Collections.Concurrent;

namespace WebProject_API_React.Server.Services
{
    public class TaskProcessingService : BackgroundService
    {
        private readonly IServiceScopeFactory _scopeFactory;
        private readonly TaskStatusBackgroundService _statusBackgroundService;
        private readonly ConcurrentQueue<BackgroundTask> _taskQueue;
        private readonly SemaphoreSlim _semaphore = new SemaphoreSlim(2, 2);
        private readonly IConfiguration _configuration;
        private readonly string _logFilePath;

        public TaskProcessingService(IServiceScopeFactory scopeFactory, TaskStatusBackgroundService statusBackgroundService, IConfiguration configuration)
        {
            _scopeFactory = scopeFactory;
            _statusBackgroundService = statusBackgroundService;
            _taskQueue = new ConcurrentQueue<BackgroundTask>();
            _configuration = configuration;

            var logsDirectory = Path.Combine(Directory.GetCurrentDirectory(), "Logs");
            Directory.CreateDirectory(logsDirectory);
            _logFilePath = Path.Combine(logsDirectory, "task_log.txt");
        }

        private async Task LogTaskInfo(string taskId, string taskStatus)
        {
            var port = _configuration.GetValue<string>("SERVER_PORT") ?? "Unknown port";

            await File.AppendAllTextAsync(_logFilePath,
                $"Task ID: {taskId}, Status: {taskStatus}, Port: {port}, Timestamp: {DateTime.Now}{Environment.NewLine}");
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            var searchTasksTask = SearchTasksInQueueAsync(stoppingToken);
            var processTasksTask = ProcessTasksInQueueAsync(stoppingToken);

            await Task.WhenAny(searchTasksTask, processTasksTask);
        }

        private async Task SearchTasksInQueueAsync(CancellationToken stoppingToken)
        {
            while (!stoppingToken.IsCancellationRequested)
            {
                using (var scope = _scopeFactory.CreateScope())
                {
                    var backgroundTaskRepository = scope.ServiceProvider.GetRequiredService<IBackgroundTaskRepository>();

                    var pendingTasks = await backgroundTaskRepository.GetListAsync(
                        t => t.Status == "Pending" && !_taskQueue.Contains(t), stoppingToken);

                    foreach (var pendingTask in pendingTasks)
                    {
                        pendingTask.Status = "InQueue";
                        pendingTask.UpdatedAt = DateTime.Now;
                        await backgroundTaskRepository.SaveChangesAsync(stoppingToken);

                        _taskQueue.Enqueue(pendingTask);
                    }
                }

                await Task.Delay(TimeSpan.FromSeconds(5), stoppingToken);
            }
        }

        private async Task ProcessTasksInQueueAsync(CancellationToken stoppingToken)
        {
            while (!stoppingToken.IsCancellationRequested)
            {
                var tasksToProcess = new List<BackgroundTask>();

                while (_taskQueue.TryDequeue(out var task))
                {
                    tasksToProcess.Add(task);
                }

                if (tasksToProcess.Count > 0)
                {
                    var processingTasks = new List<Task>();

                    foreach (var task in tasksToProcess)
                    {

                        await _semaphore.WaitAsync(stoppingToken);

                        var processingTask = ProcessSingleTask(task, stoppingToken).ContinueWith(t =>
                        {
                            _semaphore.Release();
                        });

                        processingTasks.Add(processingTask);
                    }

                    try
                    {
                        Task.WhenAll(processingTasks);
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine($"Error during task processing: {ex.Message}");
                    }
                }

                await Task.Delay(100, stoppingToken);
            }
        }



        private async Task ProcessSingleTask(BackgroundTask task, CancellationToken stoppingToken)
        {
            using (var scope = _scopeFactory.CreateScope())
            {
                var backgroundTaskRepository = scope.ServiceProvider.GetRequiredService<IBackgroundTaskRepository>();

                var dbTask = await backgroundTaskRepository.FirstOrDefaultAsync(u => u.Id == task.Id, stoppingToken);
                if (dbTask == null || dbTask.Status == "Cancelled")
                {
                    return;
                }

                dbTask.Status = "InProgress";
                dbTask.UpdatedAt = DateTime.Now;
                await backgroundTaskRepository.SaveChangesAsync(stoppingToken);
                try
                {
                    string? result = null;
                    if (task.Result != null)
                    {
                        result = task.Result;
                    }
                    task.Result = await ProcessTaskAsync(task);

                    if (task.Result.Contains("An error occurred:"))
                    {
                        dbTask.Status = "Failed";
                        dbTask.ErrorMessage = task.Result;
                        dbTask.Result = null;
                    }
                    else if (task.Result.Contains("Task was cancelled."))
                    {
                        dbTask.Result = result;
                    }
                    else
                    {
                        dbTask.Status = "Completed";
                        dbTask.Result = task.Result;
                    }
                }
                catch (Exception ex)
                {
                    dbTask.Result = null;
                    dbTask.Status = "Failed";
                    dbTask.ErrorMessage = ex.Message;
                }
                finally
                {
                    dbTask.UpdatedAt = DateTime.Now;
                    await backgroundTaskRepository.SaveChangesAsync(stoppingToken);
                }
            }
        }





        private async Task<string> ProcessTaskAsync(BackgroundTask task)
        {
            try
            {
                switch (task.TaskType)
                {
                    case "FactorialCalculation":
                        return await CalculateFactorialAsync(task);

                    case "Calculation":
                        return await CalculationAsync(task);

                    default:
                        return "Unsupported task type.";
                }
            }
            catch (Exception ex)
            {
                return $"An error occurred: {ex.Message}";
            }
        }

        private async Task<string> CalculateFactorialAsync(BackgroundTask task)
        {
            var factorialParameters = JsonSerializer.Deserialize<Dictionary<string, int>>(task.Parameters);
            int n = factorialParameters["number"];
            long result = 1;

            for (int i = 1; i <= n; i++)
            {
                if (_statusBackgroundService.IsTaskCancelled(task.Id))
                {
                    task.Status = "Cancelled";
                    return "Task was cancelled.";
                }

                result *= i;


            }
            await Task.Delay(1);
            return result.ToString();
        }

        private async Task<string> CalculationAsync(BackgroundTask task)
        {
            var calculationParameters = JsonSerializer.Deserialize<Dictionary<string, int>>(task.Parameters);
            int n = calculationParameters["number"];
            long result = 1;

            for (int i = 1; i <= n; i++)
            {
                for (int j = 1; j <= n; j++)
                {
                    for (int k = 1; k <= n; k++)
                    {
                        if (_statusBackgroundService.IsTaskCancelled(task.Id))
                        {
                            task.Status = "Cancelled";
                            return "Task was cancelled.";
                        }
                        result += 1;
                        
                    }
                }
                
            }
            await Task.Delay(1);
            return result.ToString();
        }


    }
}
