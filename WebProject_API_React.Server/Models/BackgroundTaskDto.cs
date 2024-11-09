using System.ComponentModel.DataAnnotations;

namespace WebProject_API_React.Server.Models
{
    public class TaskRequestDto
    {
        [Required(ErrorMessage = "UserId is required.")]
        public int UserId { get; set; }

        [Required(ErrorMessage = "TaskType is required.")]
        [MaxLength(50, ErrorMessage = "TaskType can't have more than 50 characters.")]
        public string TaskType { get; set; }

        public Dictionary<string, int> Parameters { get; set; }
    }
}
