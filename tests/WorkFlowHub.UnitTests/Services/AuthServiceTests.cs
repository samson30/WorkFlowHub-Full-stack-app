using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using WorkFlowHub.Core.DTOs.Auth;
using WorkFlowHub.Infrastructure.Data;
using WorkFlowHub.Infrastructure.Repositories;
using WorkFlowHub.Infrastructure.Services;
using Xunit;

namespace WorkFlowHub.UnitTests.Services;

public class AuthServiceTests : IDisposable
{
    private readonly AppDbContext _context;
    private readonly AuthService _service;

    public AuthServiceTests()
    {
        var options = new DbContextOptionsBuilder<AppDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;
        _context = new AppDbContext(options);

        var config = new ConfigurationBuilder()
            .AddInMemoryCollection(new Dictionary<string, string?>
            {
                ["Jwt:Key"]         = "unit-test-secret-key-minimum-32-characters-long",
                ["Jwt:Issuer"]      = "WorkFlowHub",
                ["Jwt:Audience"]    = "WorkFlowHubUsers",
                ["Jwt:ExpiryHours"] = "1"
            })
            .Build();

        _service = new AuthService(new UserRepository(_context), config);
    }

    [Fact]
    public async Task Register_ValidData_ReturnsTokenAndEmail()
    {
        var dto = new RegisterDto { Email = "test@example.com", Password = "Password123!" };

        var result = await _service.RegisterAsync(dto);

        Assert.NotEmpty(result.Token);
        Assert.Equal("test@example.com", result.Email);
        Assert.Equal("User", result.Role);
    }

    [Fact]
    public async Task Register_DuplicateEmail_ThrowsInvalidOperationException()
    {
        var dto = new RegisterDto { Email = "dup@example.com", Password = "Password123!" };
        await _service.RegisterAsync(dto);

        await Assert.ThrowsAsync<InvalidOperationException>(
            () => _service.RegisterAsync(dto));
    }

    [Fact]
    public async Task Register_EmailStoredAsLowercase()
    {
        var dto = new RegisterDto { Email = "UPPER@EXAMPLE.COM", Password = "Password123!" };

        var result = await _service.RegisterAsync(dto);

        Assert.Equal("upper@example.com", result.Email);
    }

    [Fact]
    public async Task Login_ValidCredentials_ReturnsToken()
    {
        await _service.RegisterAsync(new RegisterDto { Email = "login@example.com", Password = "Password123!" });

        var result = await _service.LoginAsync(new LoginDto { Email = "login@example.com", Password = "Password123!" });

        Assert.NotEmpty(result.Token);
        Assert.Equal("login@example.com", result.Email);
    }

    [Fact]
    public async Task Login_WrongPassword_ThrowsUnauthorizedAccessException()
    {
        await _service.RegisterAsync(new RegisterDto { Email = "wrong@example.com", Password = "Password123!" });

        await Assert.ThrowsAsync<UnauthorizedAccessException>(
            () => _service.LoginAsync(new LoginDto { Email = "wrong@example.com", Password = "WrongPass!" }));
    }

    [Fact]
    public async Task Login_NonexistentEmail_ThrowsUnauthorizedAccessException()
    {
        await Assert.ThrowsAsync<UnauthorizedAccessException>(
            () => _service.LoginAsync(new LoginDto { Email = "ghost@example.com", Password = "Password123!" }));
    }

    [Fact]
    public async Task Register_TokenExpiryIsSet()
    {
        var result = await _service.RegisterAsync(new RegisterDto { Email = "expiry@example.com", Password = "Password123!" });

        Assert.True(result.ExpiresAt > DateTime.UtcNow);
    }

    public void Dispose() => _context.Dispose();
}
