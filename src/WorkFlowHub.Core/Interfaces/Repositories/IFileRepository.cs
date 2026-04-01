using WorkFlowHub.Core.Models;

namespace WorkFlowHub.Core.Interfaces.Repositories;

public interface IFileRepository : IRepository<FileRecord>
{
    Task<IEnumerable<FileRecord>> GetByUserIdAsync(Guid userId);
    Task<FileRecord?> GetByFileNameAsync(string fileName);
}
