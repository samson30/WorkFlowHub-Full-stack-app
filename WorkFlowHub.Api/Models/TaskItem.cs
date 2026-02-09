using System.ComponentModel.DataAnnotations;
using WorkFlowHub.Api.Enums;

namespace WorkFlowHub.Api.Models
{
    public class TaskItem
    {
        public int Id { get; set; }

        [Required]
        public string Title { get; set; } = string.Empty;

        public string? Description { get; set; }

        public Status Status { get; set; } = Status.Todo;
        // Todo | InProgress | Done

        public DateTime CreatedAt { get; set; }

        // 🔗 Relationship
        public int ProjectId { get; set; }
        public Project Project { get; set; } = null!;
    }
}
