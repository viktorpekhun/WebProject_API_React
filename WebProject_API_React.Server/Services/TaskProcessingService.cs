using Microsoft.Extensions.Hosting;
using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
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
            Directory.CreateDirectory(logsDirectory); // Створює папку `Logs`, якщо її не існує
            _logFilePath = Path.Combine(logsDirectory, "task_log.txt");
        }

        private async Task LogTaskInfo(string taskId, string taskStatus)
        {
            // Отримуємо `applicationUrl`
            var port = _configuration.GetValue<string>("SERVER_PORT") ?? "Unknown port";

            // Записуємо в файл інформацію про завдання
            await File.AppendAllTextAsync(_logFilePath,
                $"Task ID: {taskId}, Status: {taskStatus}, Port: {port}, Timestamp: {DateTime.Now}{Environment.NewLine}");
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            // Запускаємо паралельно два завдання для пошуку нових задач та обробки черги
            var searchTasksTask = SearchTasksInQueueAsync(stoppingToken);
            var processTasksTask = ProcessTasksInQueueAsync(stoppingToken);

            // Чекаємо завершення будь-якого з двох завдань
            await Task.WhenAny(searchTasksTask, processTasksTask);
        }

        // Метод для постійного пошуку нових завдань і додавання їх у чергу
        private async Task SearchTasksInQueueAsync(CancellationToken stoppingToken)
        {
            while (!stoppingToken.IsCancellationRequested)
            {
                using (var scope = _scopeFactory.CreateScope())
                {
                    var backgroundTaskRepository = scope.ServiceProvider.GetRequiredService<IBackgroundTaskRepository>();

                    // Знаходимо нові завдання зі статусом "Pending" і додаємо їх у чергу
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

        // Метод для обробки задач у черзі
        private async Task ProcessTasksInQueueAsync(CancellationToken stoppingToken)
        {
            while (!stoppingToken.IsCancellationRequested)
            {
                var tasksToProcess = new List<BackgroundTask>();

                // Забираємо всі задачі з черги
                while (_taskQueue.TryDequeue(out var task))
                {
                    tasksToProcess.Add(task);
                }

                if (tasksToProcess.Count > 0)
                {
                    var processingTasks = new List<Task>();

                    foreach (var task in tasksToProcess)
                    {
                        // Використовуємо семафор для обмеження кількості одночасних завдань
                        await _semaphore.WaitAsync(stoppingToken); // Чекаємо на доступ до семафора

                        var processingTask = ProcessSingleTask(task, stoppingToken).ContinueWith(t =>
                        {
                            // Випускаємо семафор після завершення кожного завдання
                            _semaphore.Release();
                        });

                        processingTasks.Add(processingTask);
                    }

                    try
                    {
                        // Чекаємо завершення всіх оброблених завдань
                        Task.WhenAll(processingTasks);
                    }
                    catch (Exception ex)
                    {
                        // Логування будь-яких винятків
                        Console.WriteLine($"Error during task processing: {ex.Message}");
                    }
                }

                // Затримка для уникнення високого навантаження, якщо черга порожня
                await Task.Delay(100, stoppingToken);
            }
        }



        private async Task ProcessSingleTask(BackgroundTask task, CancellationToken stoppingToken)
        {
            using (var scope = _scopeFactory.CreateScope())
            {
                var backgroundTaskRepository = scope.ServiceProvider.GetRequiredService<IBackgroundTaskRepository>();

                // Оновіть інформацію про завдання з бази перед збереженням
                var dbTask = await backgroundTaskRepository.FirstOrDefaultAsync(u => u.Id == task.Id, stoppingToken);
                if (dbTask == null || dbTask.Status == "Cancelled")
                {
                    return; // Завдання могло бути видалене або скасоване
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
                // Перевіряємо, чи завдання було скасовано
                if (_statusBackgroundService.IsTaskCancelled(task.Id))
                {
                    task.Status = "Cancelled";
                    return "Task was cancelled.";
                }

                result *= i;

                // Імітуємо асинхронну операцію для ітерації
                
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

                        // Імітуємо асинхронну операцію для кожного вкладеного циклу
                        
                    }
                }
                
            }
            await Task.Delay(1);
            return result.ToString();
        }


    }
}
