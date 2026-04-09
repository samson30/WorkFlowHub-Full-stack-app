using WorkFlowHub.Core.DTOs.Tasks;
using WorkFlowHub.Core.Interfaces.Repositories;
using WorkFlowHub.Core.Interfaces.Services;
using WorkFlowHub.Core.Models;

namespace WorkFlowHub.Infrastructure.Services;

public class CommentService : ICommentService
{
    private readonly ICommentRepository _commentRepository;
    private readonly ITaskRepository _taskRepository;

    public CommentService(ICommentRepository commentRepository, ITaskRepository taskRepository)
    {
        _commentRepository = commentRepository;
        _taskRepository = taskRepository;
    }

    public async Task<IEnumerable<CommentResponseDto>> GetCommentsAsync(Guid taskId, Guid userId)
    {
        var task = await _taskRepository.GetByIdAsync(taskId)
            ?? throw new KeyNotFoundException("Task not found.");

        var comments = await _commentRepository.GetByTaskIdAsync(taskId);
        return comments.Select(MapToDto);
    }

    public async Task<CommentResponseDto> AddCommentAsync(Guid taskId, CreateCommentDto dto, Guid userId)
    {
        var task = await _taskRepository.GetByIdAsync(taskId)
            ?? throw new KeyNotFoundException("Task not found.");

        var comment = new TaskComment
        {
            Body = dto.Body,
            TaskId = taskId,
            AuthorId = userId
        };

        await _commentRepository.AddAsync(comment);
        var created = (await _commentRepository.GetByTaskIdAsync(taskId))
            .First(c => c.Id == comment.Id);

        return MapToDto(created);
    }

    public async Task DeleteCommentAsync(Guid commentId, Guid userId)
    {
        var comment = await _commentRepository.GetByIdAsync(commentId)
            ?? throw new KeyNotFoundException("Comment not found.");

        if (comment.AuthorId != userId)
            throw new UnauthorizedAccessException("You can only delete your own comments.");

        await _commentRepository.DeleteAsync(comment);
    }

    private static CommentResponseDto MapToDto(TaskComment c) => new()
    {
        Id = c.Id,
        Body = c.Body,
        AuthorEmail = c.Author?.Email ?? string.Empty,
        CreatedAt = c.CreatedAt
    };
}
