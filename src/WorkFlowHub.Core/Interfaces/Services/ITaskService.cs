using WorkFlowHub.Core.DTOs.Common;
using WorkFlowHub.Core.DTOs.Tasks;

namespace WorkFlowHub.Core.Interfaces.Services;

public interface ITaskService
{
    Task<PagedResult<TaskResponseDto>> GetTasksAsync(Guid projectId, Guid userId, PaginationParams pagination);
    Task<TaskResponseDto?> GetTaskByIdAsync(Guid projectId, Guid taskId, Guid userId);
    Task<TaskResponseDto> CreateTaskAsync(Guid projectId, CreateTaskDto dto, Guid userId);
    Task<TaskResponseDto> UpdateTaskAsync(Guid projectId, Guid taskId, UpdateTaskDto dto, Guid userId);
    Task<TaskResponseDto> UpdateTaskStatusAsync(Guid projectId, Guid taskId, UpdateStatusDto dto, Guid userId);
    Task DeleteTaskAsync(Guid projectId, Guid taskId, Guid userId);
}
