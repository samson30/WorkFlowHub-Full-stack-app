using WorkFlowHub.Core.Models;

namespace WorkFlowHub.Core.Interfaces.Repositories;

public interface ICommentRepository : IRepository<TaskComment>
{
    Task<IEnumerable<TaskComment>> GetByTaskIdAsync(Guid taskId);
}
