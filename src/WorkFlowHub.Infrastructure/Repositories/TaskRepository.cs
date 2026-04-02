using Microsoft.EntityFrameworkCore;
using WorkFlowHub.Core.Interfaces.Repositories;
using WorkFlowHub.Core.Models;
using WorkFlowHub.Infrastructure.Data;

namespace WorkFlowHub.Infrastructure.Repositories;

public class TaskRepository : BaseRepository<TaskItem>, ITaskRepository
{
    public TaskRepository(AppDbContext context) : base(context) { }

    public async Task<IEnumerable<TaskItem>> GetByProjectIdAsync(Guid projectId) =>
        await _dbSet
            .Include(t => t.AssignedUser)
            .Where(t => t.ProjectId == projectId)
            .OrderByDescending(t => t.CreatedAt)
            .ToListAsync();

    public async Task<TaskItem?> GetByIdWithDetailsAsync(Guid id) =>
        await _dbSet
            .Include(t => t.AssignedUser)
            .FirstOrDefaultAsync(t => t.Id == id);
}
