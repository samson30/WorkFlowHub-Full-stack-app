using WorkFlowHub.Core.Models;

namespace WorkFlowHub.Core.Interfaces.Repositories;

public interface IProjectRepository : IRepository<Project>
{
    Task<IEnumerable<Project>> GetByOwnerIdAsync(Guid ownerId);
    Task<Project?> GetByIdWithTasksAsync(Guid id);
}
