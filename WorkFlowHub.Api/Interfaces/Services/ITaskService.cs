using WorkFlowHub.Api.DTOs.Common;
using WorkFlowHub.Api.Enums;
using WorkFlowHub.Api.Models;

namespace WorkFlowHub.Api.Services
{
    public interface ITaskService
    {
        Task<PagedResult<TaskItem>> GetTasksForProjectAsync(int projectId, int userId, PaginationParams paging);
        Task<TaskItem?> CreateTaskAsync(TaskItem task, int userId);
        Task<bool> UpdateTaskAsync(int taskId, int userId, TaskItem updated);
        Task<bool> DeleteTaskAsync(int taskId, int userId);
        Task<bool> UpdateTaskStatusAsync(int taskId, int userId, Status newStatus);
    }
}
