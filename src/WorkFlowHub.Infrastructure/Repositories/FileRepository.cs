using Microsoft.EntityFrameworkCore;
using WorkFlowHub.Core.Interfaces.Repositories;
using WorkFlowHub.Core.Models;
using WorkFlowHub.Infrastructure.Data;

namespace WorkFlowHub.Infrastructure.Repositories;

public class FileRepository : BaseRepository<FileRecord>, IFileRepository
{
    public FileRepository(AppDbContext context) : base(context) { }

    public async Task<IEnumerable<FileRecord>> GetByUserIdAsync(Guid userId) =>
        await _dbSet
            .Where(f => f.UploadedBy == userId)
            .OrderByDescending(f => f.CreatedAt)
            .ToListAsync();

    public async Task<FileRecord?> GetByFileNameAsync(string fileName) =>
        await _dbSet.FirstOrDefaultAsync(f => f.FileName == fileName);
}
