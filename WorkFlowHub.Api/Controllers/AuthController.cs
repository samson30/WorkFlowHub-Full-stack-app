using Microsoft.AspNetCore.Mvc;
using WorkFlowHub.Api.DTOs;
using WorkFlowHub.Api.Interfaces.Services;

namespace WorkFlowHub.Api.Controllers;

[ApiController]
[Route("api/auth")]
public class AuthController : ControllerBase
{
    private readonly IAuthService _authService;

    public AuthController(IAuthService authService)
    {
        _authService = authService;
    }

    [HttpPost("register")]
    public async Task<IActionResult> Register(RegisterDto dto)
    {
        try
        {
            var user = await _authService.RegisterAsync(dto);
            return Ok(new { message = "User registered successfully", userId = user.Id });
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new { error = ex.Message });
        }
    }

    [HttpPost("login")]
    public async Task<IActionResult> Login(LoginDto dto)
    {
        var token = await _authService.LoginAsync(dto);
        
        if (token == null)
        {
            return Unauthorized(new { error = "Invalid email or password" });
        }

        return Ok(new { token });
    }
}
