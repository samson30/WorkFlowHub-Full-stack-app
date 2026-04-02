using Microsoft.EntityFrameworkCore;
using WorkFlowHub.Core.Interfaces.Repositories;
using WorkFlowHub.Core.Models;
using WorkFlowHub.Infrastructure.Data;

namespace WorkFlowHub.Infrastructure.Repositories;

public class ProjectRepository : BaseRepository<Project>, IProjectRepository
{
    public ProjectRepository(AppDbContext context) : base(context) { }

    public async Task<IEnumerable<Project>> GetByOwnerIdAsync(Guid ownerId) =>
        await _dbSet
            .Where(p => p.OwnerId == ownerId)
            .OrderByDescending(p => p.UpdatedAt)
            .ToListAsync();

    public async Task<Project?> GetByIdWithTasksAsync(Guid id) =>
        await _dbSet
            .Include(p => p.Tasks)
            .FirstOrDefaultAsync(p => p.Id == id);
}
