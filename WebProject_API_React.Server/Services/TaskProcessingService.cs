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
        private readonly TaskCancellationService _taskCancellationService;

        public TaskProcessingService(IServiceScopeFactory scopeFactory, TaskCancellationService taskCancellationService)
        {
            _scopeFactory = scopeFactory;
            _taskCancellationService = taskCancellationService;
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
                        
                        // Генеруємо окремий токен для кожного завдання
                        var taskCancellationToken = _taskCancellationService.GetCancellationToken(task.Id);
                        task.Status = "InProgress";
                        task.UpdatedAt = DateTime.Now;
                        await backgroundTaskRepository.SaveChangesAsync(taskCancellationToken);

                        try
                        {
                            string? result = null;
                            if (task.Result != null)
                            {
                                result = task.Result;
                            }
                            task.Result = await ProcessTaskAsync(task, taskCancellationToken);

                            
                            if (task.Result.Contains("An error occurred:"))
                            {
                                if (task.Result.Contains("The operation was canceled."))
                                {
                                    task.Status = "Canceled";
                                    task.Result = result;
                                }
                                else
                                {
                                    task.Status = "Failed";
                                    task.ErrorMessage = task.Result;
                                    task.Result = null;
                                }

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

                            // Видаляємо токен після завершення роботи із завданням
                            _taskCancellationService.TokenDispose(task.Id);
                        }
                    }
                }

                // Пауза для відновлення після завершення завдання або в разі відсутності завдань
                await Task.Delay(TimeSpan.FromSeconds(5), stoppingToken);
            }
        }


        //private async Task<string> ProcessTaskAsync(BackgroundTask task, CancellationToken cancellationToken)
        //{
        //    try
        //    {
        //        switch (task.TaskType)
        //        {
        //            case "FactorialCalculation":
        //                var factorialParameters = JsonSerializer.Deserialize<Dictionary<string, int>>(task.Parameters);
        //                int number = factorialParameters["number"];
        //                return await Task.Run(() => CalculateFactorial(number, cancellationToken).ToString(), cancellationToken);

        //            case "Calculation":
        //                var calculationParameters = JsonSerializer.Deserialize<Dictionary<string, int>>(task.Parameters);
        //                int calcNumber = calculationParameters["number"];
        //                return await Task.Run(() => Calculation(calcNumber, cancellationToken).ToString(), cancellationToken);

        //            default:
        //                return await Task.FromResult("Unsupported task type.");
        //        }
        //    }
        //    catch (TaskCanceledException)
        //    {
        //        return "Task was canceled.";
        //    }
        //    catch (Exception ex)
        //    {
        //        return $"An error occurred: {ex.Message}";
        //    }
        //}

        //private long CalculateFactorial(int n, CancellationToken cancellationToken)
        //{
        //    long result = 1;
        //    for (int i = 1; i <= n; i++)
        //        if (cancellationToken.IsCancellationRequested)
        //            throw new TaskCanceledException();
        //        else
        //            result *= i;
        //    return result;
        //}
        //private long Calculation(int n, CancellationToken cancellationToken)
        //{
        //    long result = 1;

        //    for (int i = 1; i <= n; i++)
        //    {
        //        for (int j = 1; j <= n; j++)
        //        {
        //            for (int k = 1; k <= n; k++)
        //            {
        //                if (cancellationToken.IsCancellationRequested)
        //                    throw new TaskCanceledException();
        //                else
        //                    result += 1;
        //            }
        //        }
        //    }

        //    return result;
        //}

        private async Task<string> ProcessTaskAsync(BackgroundTask task, CancellationToken cancellationToken)
        {
            try
            {
                switch (task.TaskType)
                {
                    case "FactorialCalculation":
                        var factorialParameters = JsonSerializer.Deserialize<Dictionary<string, int>>(task.Parameters);
                        int number = factorialParameters["number"];
                        return await CalculateFactorialAsync(number, cancellationToken);

                    case "Calculation":
                        var calculationParameters = JsonSerializer.Deserialize<Dictionary<string, int>>(task.Parameters);
                        int calcNumber = calculationParameters["number"];
                        return await CalculationAsync(calcNumber, cancellationToken);

                    default:
                        return await Task.FromResult("Unsupported task type.");
                }
            }
            catch (Exception ex)
            {
                return $"An error occurred: {ex.Message}";
            }

        }

        private async Task<string> CalculateFactorialAsync(int n, CancellationToken cancellationToken)
        {
            long result = 1;
            for (int i = 1; i <= n; i++)
            {
                cancellationToken.ThrowIfCancellationRequested(); // Перевірка скасування
                result *= i;

            }
            return result.ToString();
        }

        private async Task<string> CalculationAsync(int n, CancellationToken cancellationToken)
        {

            long result = 1;

            for (int i = 1; i <= n; i++)
            {
                for (int j = 1; j <= n; j++)
                {
                    for (int k = 1; k <= n; k++)
                    {
                        cancellationToken.ThrowIfCancellationRequested(); // Перевірка скасування
                        result += 1;

                    }
                }
            }

            return result.ToString();
        }


    }
}
