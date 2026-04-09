using WorkFlowHub.Core.DTOs.Search;

namespace WorkFlowHub.Core.Interfaces.Services;

public interface ISearchService
{
    Task<SearchResultDto> SearchAsync(string query, Guid userId);
}
