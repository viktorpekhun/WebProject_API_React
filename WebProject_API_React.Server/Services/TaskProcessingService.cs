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
        //private readonly string _serverId; // Unique identifier for each server

        public TaskProcessingService(IServiceScopeFactory scopeFactory, TaskStatusBackgroundService statusBackgroundService)
        {
            _scopeFactory = scopeFactory;
            _statusBackgroundService = statusBackgroundService;
            //_serverId = GenerateServerId(); // Unique identifier based on port or other server-specific data
        }


        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            while (!stoppingToken.IsCancellationRequested)
            {
                using (var scope = _scopeFactory.CreateScope())
                {
                    var backgroundTaskRepository = scope.ServiceProvider.GetRequiredService<IBackgroundTaskRepository>();
                    var task = await backgroundTaskRepository.FirstOrDefaultAsync(t => t.Status == "Pending", stoppingToken);

                    if (task != null)
                    {
                        task.Status = "InProgress";
                        task.UpdatedAt = DateTime.Now;
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

                                task.Status = "Failed";
                                task.ErrorMessage = task.Result;
                                task.Result = null;
                             
                            }
                            else if (task.Result.Contains("Task was cancelled."))
                            {
                                task.Result = result;
                            }
                            else
                            {
                                task.Status = "Completed";
                            }
                        }
                        catch (Exception ex)
                        {
                            task.Result = null;
                            task.Status = "Failed";
                            task.ErrorMessage = ex.Message;
                        }
                        finally
                        {
                            task.UpdatedAt = DateTime.Now;
                            await backgroundTaskRepository.SaveChangesAsync(stoppingToken);

                        }

                    }
                }

                await Task.Delay(TimeSpan.FromSeconds(5), stoppingToken);
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
                        return await Task.FromResult("Unsupported task type.");
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

            return result.ToString();
        }

        private string GenerateServerId()
        {
            // Example: use port or other unique identifier for this server
            return Environment.GetEnvironmentVariable("SERVER_ID") ?? Guid.NewGuid().ToString();
        }

    }
}
