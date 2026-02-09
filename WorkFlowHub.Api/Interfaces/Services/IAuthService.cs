using WorkFlowHub.Api.DTOs;
using WorkFlowHub.Api.Models;

namespace WorkFlowHub.Api.Interfaces.Services;

public interface IAuthService
{
    Task<User> RegisterAsync(RegisterDto dto);
    Task<string?> LoginAsync(LoginDto dto);
    Task<bool> UserExistsAsync(string email);
}
