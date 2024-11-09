using Microsoft.AspNetCore.Mvc;
using System.Text.Json;
using WebProject_API_React.Server.Models;
using WebProject_API_React.Server.Repository.IRepository;
using WebProject_API_React.Server.Services;

namespace WebProject_API_React.Server.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class TaskController : ControllerBase
    {
        private readonly IBackgroundTaskRepository _backgroundTaskRepository;
        private readonly TaskStatusBackgroundService _taskStatusBackgroundService;
        private readonly string _logFilePath;

        public TaskController(IBackgroundTaskRepository backgroundTaskRepository, TaskStatusBackgroundService taskStatusBackgroundService)
        {
            _backgroundTaskRepository = backgroundTaskRepository;
            _taskStatusBackgroundService = taskStatusBackgroundService;

            var logsDirectory = Path.Combine(Directory.GetCurrentDirectory(), "Logs");
            Directory.CreateDirectory(logsDirectory);
            _logFilePath = Path.Combine(logsDirectory, "task_log.txt");
        }

        private async Task LogMessageAsync(string message)
        {
            var logEntry = $"{DateTime.Now:yyyy-MM-dd HH:mm:ss} - {message}{Environment.NewLine}";
            await System.IO.File.AppendAllTextAsync(_logFilePath, logEntry);
        }

        [HttpPost("create")]
        public async Task<IActionResult> CreateTask([FromBody] TaskRequestDto taskRequestDto)
        {
            try
            {
                var task = new BackgroundTask
                {
                    UserId = taskRequestDto.UserId,
                    TaskType = taskRequestDto.TaskType,
                    Status = "Pending",
                    CreatedAt = DateTime.Now,
                    UpdatedAt = DateTime.Now,
                    Parameters = JsonSerializer.Serialize(taskRequestDto.Parameters)
                };

                await _backgroundTaskRepository.AddAsync(task);
                await _backgroundTaskRepository.SaveChangesAsync();
                await LogMessageAsync($"New task: {task.UserId}  {task.TaskType}  {task.Status}  {task.Parameters}");

                return Ok(new { taskId = task.Id, message = "Task created successfully." });
            }
            catch (Exception ex)
            {
                await LogMessageAsync($"Error creating task for user {taskRequestDto.UserId}: {ex.Message}");
                return StatusCode(500, "An error occurred while creating the task.");
            }
        }


        [HttpPost("cancel/{id}")]
        public async Task<IActionResult> CancelTask(int id)
        {
            var task = await _backgroundTaskRepository.FirstOrDefaultAsync(u => u.Id == id);
            if (task == null)
                return NotFound("Task not found.");

            if (task.Status == "Completed" || task.Status == "Failed")
                return BadRequest("Cannot cancel a completed or failed task.");




            task.Status = "Cancelled";
            task.UpdatedAt = DateTime.Now;
            await _backgroundTaskRepository.SaveChangesAsync();
            return Ok(new { taskId = id, message = "Task canceled successfully." });


            
        }

        [HttpPost("restart/{id}")]
        public async Task<IActionResult> RestartTask(int id)
        {
            var task = await _backgroundTaskRepository.FirstOrDefaultAsync(u => u.Id == id);
            if (task == null)
                return NotFound("Task not found.");


            if (task.Status == "InProgress" || task.Status == "InQueue" || task.Status == "Pending")
            {
                task.Status = "Cancelled";
                task.UpdatedAt = DateTime.Now;
                task.CreatedAt = DateTime.Now;
                await _backgroundTaskRepository.SaveChangesAsync();
                await Task.Delay(1000);
            }
            await _taskStatusBackgroundService.RestartTaskAsync(task);

            task.CreatedAt = DateTime.Now;
            await _backgroundTaskRepository.SaveChangesAsync();
            return Ok(new { taskId = id, message = "Task restarted successfully." });
        }


        [HttpGet("result/{id}")]
        public async Task<IActionResult> GetTaskResult(int id)
        {
            var task = await _backgroundTaskRepository.FirstOrDefaultAsync(u => u.Id == id);
            if (task == null)
            {
                return NotFound(new { message = "Task not found." });
            }

            return Ok(new
            {
                TaskId = task.Id,
                TaskStatus = task.Status,
                CreatedAt = task.CreatedAt,
                UpdatedAt = task.UpdatedAt,
                Result = task.Result,
                ErrorMessage = task.ErrorMessage,
                Parameters = task.Parameters
            });
        }

        [HttpGet("status/{id}")]
        public async Task<IActionResult> GetTaskStatus(int id)
        {
            var task = await _backgroundTaskRepository.FirstOrDefaultAsync(u => u.Id == id);
            if (task == null)
                return NotFound("Task not found.");

            return Ok(new { task.Id, task.Status, task.Result, task.ErrorMessage });
        }

        [HttpGet("history")]
        public async Task<IActionResult> GetTaskHistory(int userId, DateTime? lastCreatedAt = null, int batchSize = 10)
        {
            var tasks = await _backgroundTaskRepository.GetListAsync(
                task => task.UserId == userId && (!lastCreatedAt.HasValue || task.CreatedAt < lastCreatedAt.Value)
            );

            var sortedTasks = tasks
                .OrderByDescending(task => task.CreatedAt)
                .Take(batchSize)
                .Select(task => new
                {
                    task.Id,
                    task.TaskType,
                    task.Status,
                    task.CreatedAt,
                    task.Result
                })
                .ToList();

            return Ok(sortedTasks);
        }


    }
}
