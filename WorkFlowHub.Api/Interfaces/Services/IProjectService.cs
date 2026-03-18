using WorkFlowHub.Api.DTOs.Common;
using WorkFlowHub.Api.Models;

namespace WorkFlowHub.Api.Services
{
    public interface IProjectService
    {
        Task<PagedResult<Project>> GetProjectsForUserAsync(int userId, PaginationParams paging);
        Task<Project?> GetProjectByIdAsync(int projectId, int userId);
        Task<Project> CreateProjectAsync(Project project);
        Task<bool> UpdateProjectAsync(int projectId, int userId, Project updated);
        Task<bool> DeleteProjectAsync(int projectId, int userId);
    }
}
