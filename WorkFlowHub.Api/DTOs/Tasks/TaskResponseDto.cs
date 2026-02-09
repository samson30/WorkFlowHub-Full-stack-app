using WorkFlowHub.Api.Enums;
using WorkFlowHub.Api.Models;

namespace WorkFlowHub.Api.DTOs
{
    public class TaskResponseDto
    {
        public int Id { get; set; }
        public string Title { get; set; } = string.Empty;
        public string? Description { get; set; }
        public Status Status { get; set; } = Status.Todo;
        public DateTime CreatedAt { get; set; }
        public int ProjectId { get; set; }
    }
}
