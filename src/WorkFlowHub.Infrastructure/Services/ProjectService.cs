using WorkFlowHub.Core.DTOs.Common;
using WorkFlowHub.Core.DTOs.Projects;
using WorkFlowHub.Core.Interfaces.Repositories;
using WorkFlowHub.Core.Interfaces.Services;
using WorkFlowHub.Core.Models;

namespace WorkFlowHub.Infrastructure.Services;

public class ProjectService : IProjectService
{
    private readonly IProjectRepository _projectRepository;

    public ProjectService(IProjectRepository projectRepository)
    {
        _projectRepository = projectRepository;
    }

    public async Task<PagedResult<ProjectResponseDto>> GetProjectsAsync(Guid userId, PaginationParams pagination)
    {
        var all = (await _projectRepository.GetByOwnerIdAsync(userId)).ToList();

        var items = all
            .Skip((pagination.PageNumber - 1) * pagination.PageSize)
            .Take(pagination.PageSize)
            .Select(MapToDto)
            .ToList();

        return new PagedResult<ProjectResponseDto>
        {
            Items = items,
            TotalCount = all.Count,
            PageNumber = pagination.PageNumber,
            PageSize = pagination.PageSize
        };
    }

    public async Task<ProjectResponseDto?> GetProjectByIdAsync(Guid id, Guid userId)
    {
        var project = await _projectRepository.GetByIdWithTasksAsync(id);
        if (project == null || project.OwnerId != userId) return null;
        return MapToDto(project);
    }

    public async Task<ProjectResponseDto> CreateProjectAsync(CreateProjectDto dto, Guid userId)
    {
        var project = new Project
        {
            Name = dto.Name,
            Description = dto.Description,
            OwnerId = userId
        };

        await _projectRepository.AddAsync(project);
        return MapToDto(project);
    }

    public async Task<ProjectResponseDto> UpdateProjectAsync(Guid id, UpdateProjectDto dto, Guid userId)
    {
        var project = await _projectRepository.GetByIdAsync(id)
            ?? throw new KeyNotFoundException("Project not found.");

        if (project.OwnerId != userId)
            throw new UnauthorizedAccessException("You do not own this project.");

        project.Name = dto.Name;
        project.Description = dto.Description;
        project.UpdatedAt = DateTime.UtcNow;

        await _projectRepository.UpdateAsync(project);
        return MapToDto(project);
    }

    public async Task DeleteProjectAsync(Guid id, Guid userId)
    {
        var project = await _projectRepository.GetByIdAsync(id)
            ?? throw new KeyNotFoundException("Project not found.");

        if (project.OwnerId != userId)
            throw new UnauthorizedAccessException("You do not own this project.");

        project.IsDeleted = true;
        project.UpdatedAt = DateTime.UtcNow;
        await _projectRepository.UpdateAsync(project);
    }

    private static ProjectResponseDto MapToDto(Project p) => new()
    {
        Id = p.Id,
        Name = p.Name,
        Description = p.Description,
        OwnerId = p.OwnerId,
        CreatedAt = p.CreatedAt,
        UpdatedAt = p.UpdatedAt,
        TaskCount = p.Tasks?.Count(t => !t.IsDeleted) ?? 0
    };
}
