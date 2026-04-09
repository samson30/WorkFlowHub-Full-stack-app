using WorkFlowHub.Core.Models;

namespace WorkFlowHub.Core.Interfaces.Repositories;

public interface ITaskRepository : IRepository<TaskItem>
{
    Task<IEnumerable<TaskItem>> GetByProjectIdAsync(Guid projectId);
    Task<TaskItem?> GetByIdWithDetailsAsync(Guid id);
    Task<TaskLabel?> GetLabelAsync(Guid labelId);
    Task AddLabelAsync(TaskLabel label);
    Task RemoveLabelAsync(TaskLabel label);
}
