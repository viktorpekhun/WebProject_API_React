using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace WebProject_API_React.Server.Models
{
    public class BackgroundTask
    {
        [Key]
        public int Id { get; set; }

        [ForeignKey("User")]
        public int UserId { get; set; }
        [Required]
        public string TaskType { get; set; }
        [Required]
        public string Status { get; set; }
        [Required]
        public DateTime CreatedAt { get; set; }
        [Required]
        public DateTime UpdatedAt { get; set; }


        public string? Result { get; set; }
        public string? ErrorMessage { get; set; }


        [Required]
        public string Parameters { get; set; }

        public virtual User User { get; set; }
    }
}
