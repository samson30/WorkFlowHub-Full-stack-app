using WorkFlowHub.Core.Enums;

namespace WorkFlowHub.Core.DTOs.Users;

public class UserResponseDto
{
    public Guid Id { get; set; }
    public string Email { get; set; } = string.Empty;
    public UserRole Role { get; set; }
    public DateTime CreatedAt { get; set; }
}
