using System.ComponentModel.DataAnnotations;
using WorkFlowHub.Core.Enums;

namespace WorkFlowHub.Core.DTOs.Tasks;

public class CreateTaskDto
{
    [Required]
    [MaxLength(300)]
    public string Title { get; set; } = string.Empty;

    [MaxLength(2000)]
    public string Description { get; set; } = string.Empty;

    public TaskPriority Priority { get; set; } = TaskPriority.Medium;
    public Guid? AssignedUserId { get; set; }
    public DateTime? DueDate { get; set; }
}
