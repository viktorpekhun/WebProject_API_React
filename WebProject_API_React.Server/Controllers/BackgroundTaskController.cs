using Microsoft.AspNetCore.Mvc;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
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


        public TaskController(IBackgroundTaskRepository backgroundTaskRepository, TaskStatusBackgroundService taskStatusBackgroundService)
        {
            _backgroundTaskRepository = backgroundTaskRepository;
            _taskStatusBackgroundService = taskStatusBackgroundService;
        }

        [HttpPost("create")]
        public async Task<IActionResult> CreateTask([FromBody] TaskRequestDto taskRequestDto)
        {
            var task = new BackgroundTask
            {
                UserId = taskRequestDto.UserId,
                TaskType = taskRequestDto.TaskType,
                Status = "Pending",
                CreatedAt = DateTime.Now,
                UpdatedAt = DateTime.Now,
                Parameters = JsonSerializer.Serialize(taskRequestDto.Parameters) // Збереження параметрів у JSON форматі
            };

            await _backgroundTaskRepository.AddAsync(task);
            await _backgroundTaskRepository.SaveChangesAsync();


            return Ok(new { taskId = task.Id, message = "Task created successfully." });
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

            // Скасовуємо завдання, якщо воно в стані InProgress
            //if (task.Status == "InProgress")
            //{
            //    var result = _taskCancellationService.CancelTask(id);
            //    if (!result)
            //    {
            //        return BadRequest(new { taskId = id, message = "Cannot cancel the ongoing task." });
            //    }
            //}

            // Оновлюємо статус і створюємо новий токен
            await _taskStatusBackgroundService.RestartTaskAsync(task);

            // Створюємо новий CancellationToken для завдання

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

            // Повертаємо статус і результат
            return Ok(new
            {
                Status = task.Status,
                Result = task.Result,
                ErrorMessage = task.ErrorMessage
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
    }
}
