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
            .Include(t => t.Labels)
            .Where(t => t.ProjectId == projectId)
            .OrderByDescending(t => t.CreatedAt)
            .ToListAsync();

    public async Task<TaskItem?> GetByIdWithDetailsAsync(Guid id) =>
        await _dbSet
            .Include(t => t.AssignedUser)
            .Include(t => t.Labels)
            .FirstOrDefaultAsync(t => t.Id == id);

    public async Task<TaskLabel?> GetLabelAsync(Guid labelId) =>
        await _context.Labels.FirstOrDefaultAsync(l => l.Id == labelId);

    public async Task AddLabelAsync(TaskLabel label)
    {
        _context.Labels.Add(label);
        await _context.SaveChangesAsync();
    }

    public async Task RemoveLabelAsync(TaskLabel label)
    {
        _context.Labels.Remove(label);
        await _context.SaveChangesAsync();
    }
}
