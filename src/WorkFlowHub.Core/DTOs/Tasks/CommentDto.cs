using System.ComponentModel.DataAnnotations;

namespace WorkFlowHub.Core.DTOs.Tasks;

public class CreateCommentDto
{
    [Required]
    [MaxLength(2000)]
    public string Body { get; set; } = string.Empty;
}

public class CommentResponseDto
{
    public Guid Id { get; set; }
    public string Body { get; set; } = string.Empty;
    public string AuthorEmail { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; }
}
