namespace WorkFlowHub.Core.DTOs.Search;

public class SearchResultDto
{
    public List<SearchItem> Projects { get; set; } = new();
    public List<SearchItem> Tasks { get; set; } = new();
}

public class SearchItem
{
    public Guid Id { get; set; }
    public string Title { get; set; } = string.Empty;
    public string? Subtitle { get; set; }
    public Guid? ProjectId { get; set; }
}
