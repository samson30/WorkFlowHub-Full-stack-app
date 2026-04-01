using WorkFlowHub.Core.DTOs.Common;
using WorkFlowHub.Core.DTOs.Projects;

namespace WorkFlowHub.Core.Interfaces.Services;

public interface IProjectService
{
    Task<PagedResult<ProjectResponseDto>> GetProjectsAsync(Guid userId, PaginationParams pagination);
    Task<ProjectResponseDto?> GetProjectByIdAsync(Guid id, Guid userId);
    Task<ProjectResponseDto> CreateProjectAsync(CreateProjectDto dto, Guid userId);
    Task<ProjectResponseDto> UpdateProjectAsync(Guid id, UpdateProjectDto dto, Guid userId);
    Task DeleteProjectAsync(Guid id, Guid userId);
}
