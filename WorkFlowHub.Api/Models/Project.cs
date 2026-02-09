using System.ComponentModel.DataAnnotations;

namespace WorkFlowHub.Api.Models
{
    public class Project
    {
        public int Id { get; set; }

        [Required]
        [MaxLength(120)]
        public string Name { get; set; } = string.Empty;

        [MaxLength(500)]
        public string? Description { get; set; }

        [Required]
        public string Status { get; set; } = "Active"; // Active | Completed

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        // Owner (User)
        public int UserId { get; set; }
        public User? User { get; set; }

public List<TaskItem> Tasks { get; set; } = new();

    }
}
