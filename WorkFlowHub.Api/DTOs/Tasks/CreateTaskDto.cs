using System.ComponentModel.DataAnnotations;
using WorkFlowHub.Api.Enums;

namespace WorkFlowHub.Api.DTOs
{
    public class CreateTaskDto
    {
        [Required(ErrorMessage = "Task title is required")]
        [StringLength(200, MinimumLength = 3, ErrorMessage = "Task title must be between 3 and 200 characters")]
        public string Title { get; set; } = string.Empty;

        [MaxLength(1000, ErrorMessage = "Description cannot exceed 1000 characters")]
        public string? Description { get; set; }

        public Status Status { get; set; } = Status.Todo;
    }
}
