using Microsoft.EntityFrameworkCore;
using WorkFlowHub.Api.Data;
using WorkFlowHub.Api.DTOs.Common;
using WorkFlowHub.Api.Models;

namespace WorkFlowHub.Api.Services
{
    public class ProjectService : IProjectService
    {
        private readonly AppDbContext _db;

        public ProjectService(AppDbContext db)
        {
            _db = db;
        }

        public async Task<PagedResult<Project>> GetProjectsForUserAsync(int userId, PaginationParams paging)
        {
            var query = _db.Projects
                .Where(p => p.UserId == userId)
                .OrderByDescending(p => p.CreatedAt);

            var totalCount = await query.CountAsync();

            var items = await query
                .Skip((paging.Page - 1) * paging.PageSize)
                .Take(paging.PageSize)
                .ToListAsync();

            return new PagedResult<Project>
            {
                Items = items,
                TotalCount = totalCount,
                Page = paging.Page,
                PageSize = paging.PageSize
            };
        }

        public async Task<Project?> GetProjectByIdAsync(int projectId, int userId)
        {
            return await _db.Projects
                .FirstOrDefaultAsync(p => p.Id == projectId && p.UserId == userId);
        }

        public async Task<Project> CreateProjectAsync(Project project)
        {
            _db.Projects.Add(project);
            await _db.SaveChangesAsync();
            return project;
        }

        public async Task<bool> UpdateProjectAsync(int projectId, int userId, Project updated)
        {
            var project = await GetProjectByIdAsync(projectId, userId);
            if (project == null) return false;

            project.Name = updated.Name;
            project.Description = updated.Description;
            project.Status = updated.Status;

            await _db.SaveChangesAsync();
            return true;
        }

        public async Task<bool> DeleteProjectAsync(int projectId, int userId)
        {
            var project = await GetProjectByIdAsync(projectId, userId);
            if (project == null) return false;

            _db.Projects.Remove(project);
            await _db.SaveChangesAsync();
            return true;
        }
    }
}
