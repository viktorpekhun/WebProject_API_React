using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace WebProject_API_React.Server.Models
{
    public class BackgroundTask
    {
        [Key]
        public int Id { get; set; }

        [ForeignKey("User")]
        public int UserId { get; set; } // Ідентифікатор користувача, який створив завдання
        [Required]
        public string TaskType { get; set; } // Тип завдання, наприклад "FactorialCalculation"
        [Required]
        public string Status { get; set; } // "Pending", "InProgress", "Completed", "Failed"
        [Required]
        public DateTime CreatedAt { get; set; }
        [Required]
        public DateTime UpdatedAt { get; set; }


        public string? Result { get; set; } // Для зберігання результату
        public string? ErrorMessage { get; set; } // Для повідомлення про помилки, якщо є


        [Required]
        public string Parameters { get; set; }

        // Навігаційна властивість для зв'язку з користувачем
        public virtual User User { get; set; }
    }
}
