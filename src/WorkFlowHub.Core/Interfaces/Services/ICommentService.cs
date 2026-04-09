using WorkFlowHub.Core.DTOs.Tasks;

namespace WorkFlowHub.Core.Interfaces.Services;

public interface ICommentService
{
    Task<IEnumerable<CommentResponseDto>> GetCommentsAsync(Guid taskId, Guid userId);
    Task<CommentResponseDto> AddCommentAsync(Guid taskId, CreateCommentDto dto, Guid userId);
    Task DeleteCommentAsync(Guid commentId, Guid userId);
}
