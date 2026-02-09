using WorkFlowHub.Api.Models;

namespace WorkFlowHub.Api.Services
{
    public interface IProjectService
    {
        Task<List<Project>> GetProjectsForUserAsync(int userId);
        Task<Project?> GetProjectByIdAsync(int projectId, int userId);
        Task<Project> CreateProjectAsync(Project project);
        Task<bool> UpdateProjectAsync(int projectId, int userId, Project updated);
        Task<bool> DeleteProjectAsync(int projectId, int userId);
    }
}
