namespace WorkFlowHub.Core.Models;

public class TaskLabel
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public Guid TaskId { get; set; }
    public TaskItem Task { get; set; } = null!;
    public string Name { get; set; } = string.Empty;
    public string Color { get; set; } = "#6366f1";
}
