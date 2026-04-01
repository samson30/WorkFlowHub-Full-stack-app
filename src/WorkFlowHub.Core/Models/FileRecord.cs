namespace WorkFlowHub.Core.Models;

public class FileRecord
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public string FileName { get; set; } = string.Empty;
    public string BlobUrl { get; set; } = string.Empty;
    public Guid UploadedBy { get; set; }
    public User Uploader { get; set; } = null!;
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}
