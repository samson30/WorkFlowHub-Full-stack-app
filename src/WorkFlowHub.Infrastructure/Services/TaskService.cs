using WorkFlowHub.Core.DTOs.Common;
using WorkFlowHub.Core.DTOs.Tasks;
using WorkFlowHub.Core.Interfaces.Repositories;
using WorkFlowHub.Core.Interfaces.Services;
using WorkFlowHub.Core.Models;
using TaskStatus = WorkFlowHub.Core.Enums.TaskStatus;

namespace WorkFlowHub.Infrastructure.Services;

public class TaskService : ITaskService
{
    private readonly ITaskRepository _taskRepository;
    private readonly IProjectRepository _projectRepository;

    public TaskService(ITaskRepository taskRepository, IProjectRepository projectRepository)
    {
        _taskRepository = taskRepository;
        _projectRepository = projectRepository;
    }

    public async Task<PagedResult<TaskResponseDto>> GetTasksAsync(Guid projectId, Guid userId, PaginationParams pagination)
    {
        await EnsureProjectOwnershipAsync(projectId, userId);

        var all = (await _taskRepository.GetByProjectIdAsync(projectId)).ToList();

        var items = all
            .Skip((pagination.PageNumber - 1) * pagination.PageSize)
            .Take(pagination.PageSize)
            .Select(MapToDto)
            .ToList();

        return new PagedResult<TaskResponseDto>
        {
            Items = items,
            TotalCount = all.Count,
            PageNumber = pagination.PageNumber,
            PageSize = pagination.PageSize
        };
    }

    public async Task<TaskResponseDto?> GetTaskByIdAsync(Guid projectId, Guid taskId, Guid userId)
    {
        await EnsureProjectOwnershipAsync(projectId, userId);

        var task = await _taskRepository.GetByIdWithDetailsAsync(taskId);
        if (task == null || task.ProjectId != projectId) return null;
        return MapToDto(task);
    }

    public async Task<TaskResponseDto> CreateTaskAsync(Guid projectId, CreateTaskDto dto, Guid userId)
    {
        await EnsureProjectOwnershipAsync(projectId, userId);

        var task = new TaskItem
        {
            Title = dto.Title,
            Description = dto.Description,
            Priority = dto.Priority,
            ProjectId = projectId,
            AssignedUserId = dto.AssignedUserId,
            DueDate = dto.DueDate
        };

        await _taskRepository.AddAsync(task);

        var created = await _taskRepository.GetByIdWithDetailsAsync(task.Id);
        return MapToDto(created!);
    }

    public async Task<TaskResponseDto> UpdateTaskAsync(Guid projectId, Guid taskId, UpdateTaskDto dto, Guid userId)
    {
        await EnsureProjectOwnershipAsync(projectId, userId);

        var task = await _taskRepository.GetByIdWithDetailsAsync(taskId)
            ?? throw new KeyNotFoundException("Task not found.");

        if (task.ProjectId != projectId)
            throw new KeyNotFoundException("Task not found.");

        task.Title = dto.Title;
        task.Description = dto.Description;
        task.Priority = dto.Priority;
        task.AssignedUserId = dto.AssignedUserId;
        task.DueDate = dto.DueDate;

        await _taskRepository.UpdateAsync(task);

        var updated = await _taskRepository.GetByIdWithDetailsAsync(task.Id);
        return MapToDto(updated!);
    }

    public async Task<TaskResponseDto> UpdateTaskStatusAsync(Guid projectId, Guid taskId, UpdateStatusDto dto, Guid userId)
    {
        await EnsureProjectOwnershipAsync(projectId, userId);

        var task = await _taskRepository.GetByIdWithDetailsAsync(taskId)
            ?? throw new KeyNotFoundException("Task not found.");

        if (task.ProjectId != projectId)
            throw new KeyNotFoundException("Task not found.");

        task.Status = dto.Status;
        await _taskRepository.UpdateAsync(task);
        return MapToDto(task);
    }

    public async Task DeleteTaskAsync(Guid projectId, Guid taskId, Guid userId)
    {
        await EnsureProjectOwnershipAsync(projectId, userId);

        var task = await _taskRepository.GetByIdAsync(taskId)
            ?? throw new KeyNotFoundException("Task not found.");

        if (task.ProjectId != projectId)
            throw new KeyNotFoundException("Task not found.");

        task.IsDeleted = true;
        await _taskRepository.UpdateAsync(task);
    }

    private async Task EnsureProjectOwnershipAsync(Guid projectId, Guid userId)
    {
        var project = await _projectRepository.GetByIdAsync(projectId)
            ?? throw new KeyNotFoundException("Project not found.");

        if (project.OwnerId != userId)
            throw new UnauthorizedAccessException("You do not own this project.");
    }

    private static TaskResponseDto MapToDto(TaskItem t) => new()
    {
        Id = t.Id,
        Title = t.Title,
        Description = t.Description,
        Status = t.Status,
        Priority = t.Priority,
        ProjectId = t.ProjectId,
        AssignedUserId = t.AssignedUserId,
        AssignedUserEmail = t.AssignedUser?.Email,
        DueDate = t.DueDate,
        CreatedAt = t.CreatedAt
    };
}
