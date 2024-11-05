using Microsoft.Extensions.Hosting;
using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using WebProject_API_React.Server.Repository.IRepository;
using WebProject_API_React.Server.Models;
using System.Text.Json;

namespace WebProject_API_React.Server.Services
{
    public class TaskProcessingService : BackgroundService
    {
        private readonly IServiceScopeFactory _scopeFactory;

        public TaskProcessingService(IServiceScopeFactory scopeFactory)
        {
            _scopeFactory = scopeFactory;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            while (!stoppingToken.IsCancellationRequested)
            {
                using (var scope = _scopeFactory.CreateScope())
                {
                    // Отримуємо інстанцію IBackgroundTaskRepository з нового скоупу
                    var backgroundTaskRepository = scope.ServiceProvider.GetRequiredService<IBackgroundTaskRepository>();

                    // Отримуємо перше завдання зі статусом Pending
                    var task = await backgroundTaskRepository.FirstOrDefaultAsync(t => t.Status == "Pending", stoppingToken);

                    if (task != null)
                    {
                        // Оновлюємо статус на "InProgress" і зберігаємо в базі
                        task.Status = "InProgress";
                        task.UpdatedAt = DateTime.Now;
                        await backgroundTaskRepository.SaveChangesAsync(stoppingToken);

                        try
                        {
                            // Обробка завдання (наприклад, розрахунок факторіалу)
                            task.Result = await ProcessTaskAsync(task);
                            task.Status = "Completed";
                        }
                        catch (Exception ex)
                        {
                            // Логіка обробки помилок
                            task.Status = "Failed";
                            task.ErrorMessage = ex.Message;
                        }

                        task.UpdatedAt = DateTime.Now;
                        await backgroundTaskRepository.SaveChangesAsync(stoppingToken);
                    }
                }

                // Пауза перед наступною перевіркою
                await Task.Delay(TimeSpan.FromSeconds(5), stoppingToken);
            }
        }

        private async Task<string> ProcessTaskAsync(BackgroundTask task)
        {

            switch (task.TaskType)
            {
                case "FactorialCalculation":
                    // Приклад парсингу параметрів
                    var factorialParameters = JsonSerializer.Deserialize<Dictionary<string, int>>(task.Parameters);
                    int number = factorialParameters["number"]; // Наприклад, отримаємо число для факторіалу
                    return await Task.Run(() => CalculateFactorial(number).ToString());
                case "Calculation":
                    // Приклад парсингу параметрів
                    var calculationParameters = JsonSerializer.Deserialize<Dictionary<string, int>>(task.Parameters);
                    int calcNumber = calculationParameters["number"]; // Наприклад, отримаємо число для факторіалу
                    return await Task.Run(() => Calculation(calcNumber).ToString());
                default:
                    return await Task.FromResult("Unsupported task type.");
            }
        }

        private long CalculateFactorial(int n)
        {
            long result = 1;
            for (int i = 1; i <= n; i++)
                result *= i;
            return result;
        }
        private long Calculation(int n)
        {
            long result = 1;
            for (int i = 1; i <= n; i++)
            {
                for (int j = 1; j <= n; j++)
                {
                    for (int k = 1; k <= n; k++)
                    {
                        result += 1;
                    }
                }
            }

            return result;
        }
    }
}
