using Microsoft.Azure.Functions.Worker;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using WorkFlowHub.Infrastructure.Data;

namespace WorkFlowHub.Functions;

public class CleanupFunction
{
    private readonly AppDbContext _context;
    private readonly ILogger<CleanupFunction> _logger;

    public CleanupFunction(AppDbContext context, ILogger<CleanupFunction> logger)
    {
        _context = context;
        _logger = logger;
    }

    /// <summary>
    /// Runs every day at midnight UTC.
    /// Permanently removes projects and tasks that were soft-deleted more than 30 days ago.
    /// </summary>
    [Function("CleanupSoftDeletedRecords")]
    public async Task Run([TimerTrigger("0 0 0 * * *")] TimerInfo timerInfo)
    {
        _logger.LogInformation("Cleanup function triggered at {Time}", DateTime.UtcNow);

        var cutoff = DateTime.UtcNow.AddDays(-30);

        var staleProjects = await _context.Projects
            .IgnoreQueryFilters()
            .Where(p => p.IsDeleted && p.UpdatedAt < cutoff)
            .ToListAsync();

        var staleTasks = await _context.Tasks
            .IgnoreQueryFilters()
            .Where(t => t.IsDeleted && t.CreatedAt < cutoff)
            .ToListAsync();

        if (staleProjects.Count > 0)
            _context.Projects.RemoveRange(staleProjects);

        if (staleTasks.Count > 0)
            _context.Tasks.RemoveRange(staleTasks);

        if (staleProjects.Count > 0 || staleTasks.Count > 0)
            await _context.SaveChangesAsync();

        _logger.LogInformation(
            "Cleanup complete. Removed {Projects} projects and {Tasks} tasks.",
            staleProjects.Count, staleTasks.Count);
    }
}
