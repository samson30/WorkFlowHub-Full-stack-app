using Microsoft.EntityFrameworkCore;
using WorkFlowHub.Core.Interfaces.Repositories;
using WorkFlowHub.Core.Models;
using WorkFlowHub.Infrastructure.Data;

namespace WorkFlowHub.Infrastructure.Repositories;

public class UserRepository : BaseRepository<User>, IUserRepository
{
    public UserRepository(AppDbContext context) : base(context) { }

    public async Task<User?> GetByEmailAsync(string email) =>
        await _dbSet.FirstOrDefaultAsync(u => u.Email == email.ToLower());

    public async Task<bool> EmailExistsAsync(string email) =>
        await _dbSet.AnyAsync(u => u.Email == email.ToLower());
}
