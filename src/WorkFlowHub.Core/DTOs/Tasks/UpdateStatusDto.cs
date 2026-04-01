using System.ComponentModel.DataAnnotations;
using WorkFlowHub.Core.Enums;
using TaskStatus = WorkFlowHub.Core.Enums.TaskStatus;

namespace WorkFlowHub.Core.DTOs.Tasks;

public class UpdateStatusDto
{
    [Required]
    public TaskStatus Status { get; set; }
}
