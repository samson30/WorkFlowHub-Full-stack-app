using Microsoft.EntityFrameworkCore;
using WorkFlowHub.Api.Data;
using WorkFlowHub.Api.Enums;
using WorkFlowHub.Api.Models;

namespace WorkFlowHub.Api.Services
{
    public class TaskService : ITaskService
    {
        private readonly AppDbContext _db;

        public TaskService(AppDbContext db)
        {
            _db = db;
        }

        public async Task<List<TaskItem>> GetTasksForProjectAsync(int projectId, int userId)
        {
            return await _db.Tasks
                .Include(t => t.Project)
                .Where(t => t.ProjectId == projectId && t.Project.UserId == userId)
                .ToListAsync();
        }

        public async Task<TaskItem?> CreateTaskAsync(TaskItem task, int userId)
        {
            var project = await _db.Projects
                .FirstOrDefaultAsync(p => p.Id == task.ProjectId && p.UserId == userId);

            if (project == null) return null;

            task.CreatedAt = DateTime.UtcNow;
            _db.Tasks.Add(task);
            await _db.SaveChangesAsync();

            return task;
        }

        public async Task<bool> UpdateTaskAsync(int taskId, int userId, TaskItem updated)
        {
            var task = await _db.Tasks
                .Include(t => t.Project)
                .FirstOrDefaultAsync(t => t.Id == taskId && t.Project.UserId == userId);

            if (task == null) return false;
            if (task.Status == Status.Done && updated.Status != Status.Done)
            {
                throw new InvalidOperationException("Completed tasks cannot be reopened.");
            }

            task.Title = updated.Title;
            task.Description = updated.Description;
            task.Status = updated.Status;

            await _db.SaveChangesAsync();
            return true;
        }

        public async Task<bool> DeleteTaskAsync(int taskId, int userId)
        {
            var task = await _db.Tasks
                .Include(t => t.Project)
                .FirstOrDefaultAsync(t => t.Id == taskId && t.Project.UserId == userId);

            if (task == null) return false;

            _db.Tasks.Remove(task);
            await _db.SaveChangesAsync();
            return true;
        }


        public async Task<bool> UpdateTaskStatusAsync(int taskId, int userId, Status newStatus)
{
    var task = await _db.Tasks
        .Include(t => t.Project)
        .FirstOrDefaultAsync(t => t.Id == taskId && t.Project.UserId == userId);

    if (task == null) return false;

    task.Status = newStatus;
    await _db.SaveChangesAsync();
    return true;
}

    }

    
}
