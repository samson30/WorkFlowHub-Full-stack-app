using Microsoft.EntityFrameworkCore;
using WorkFlowHub.Core.DTOs.Search;
using WorkFlowHub.Core.Interfaces.Services;
using WorkFlowHub.Infrastructure.Data;

namespace WorkFlowHub.Infrastructure.Services;

public class SearchService : ISearchService
{
    private readonly AppDbContext _context;

    public SearchService(AppDbContext context)
    {
        _context = context;
    }

    public async Task<SearchResultDto> SearchAsync(string query, Guid userId)
    {
        var q = query.Trim().ToLower();

        var projects = await _context.Set<WorkFlowHub.Core.Models.Project>()
            .Where(p => p.OwnerId == userId && p.Name.ToLower().Contains(q))
            .OrderByDescending(p => p.UpdatedAt)
            .Take(5)
            .Select(p => new SearchItem
            {
                Id = p.Id,
                Title = p.Name,
                Subtitle = p.Description.Length > 60 ? p.Description.Substring(0, 60) + "…" : p.Description,
            })
            .ToListAsync();

        var tasks = await _context.Set<WorkFlowHub.Core.Models.TaskItem>()
            .Include(t => t.Project)
            .Where(t => t.Project.OwnerId == userId && t.Title.ToLower().Contains(q))
            .OrderByDescending(t => t.CreatedAt)
            .Take(5)
            .Select(t => new SearchItem
            {
                Id = t.Id,
                Title = t.Title,
                Subtitle = t.Project.Name,
                ProjectId = t.ProjectId,
            })
            .ToListAsync();

        return new SearchResultDto { Projects = projects, Tasks = tasks };
    }
}
