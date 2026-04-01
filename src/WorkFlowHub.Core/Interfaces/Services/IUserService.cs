using WorkFlowHub.Core.DTOs.Users;

namespace WorkFlowHub.Core.Interfaces.Services;

public interface IUserService
{
    Task<UserResponseDto?> GetCurrentUserAsync(Guid userId);
}
