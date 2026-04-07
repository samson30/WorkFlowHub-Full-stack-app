using System.Net;
using System.Net.Http.Json;
using WorkFlowHub.Core.DTOs.Auth;
using Xunit;

namespace WorkFlowHub.IntegrationTests;

public class AuthIntegrationTests : IClassFixture<WorkFlowHubWebApplicationFactory>
{
    private readonly HttpClient _client;

    public AuthIntegrationTests(WorkFlowHubWebApplicationFactory factory)
    {
        _client = factory.CreateClient();
    }

    [Fact]
    public async Task Register_ValidData_Returns201WithToken()
    {
        var dto = new RegisterDto
        {
            Email    = $"integ_{Guid.NewGuid()}@test.com",
            Password = "Integration@123"
        };

        var response = await _client.PostAsJsonAsync("/api/auth/register", dto);

        Assert.Equal(HttpStatusCode.Created, response.StatusCode);
        var result = await response.Content.ReadFromJsonAsync<AuthResponseDto>();
        Assert.NotNull(result);
        Assert.NotEmpty(result!.Token);
        Assert.Equal(dto.Email, result.Email);
    }

    [Fact]
    public async Task Register_DuplicateEmail_Returns400()
    {
        var dto = new RegisterDto
        {
            Email    = $"dup_{Guid.NewGuid()}@test.com",
            Password = "Integration@123"
        };

        await _client.PostAsJsonAsync("/api/auth/register", dto);
        var response = await _client.PostAsJsonAsync("/api/auth/register", dto);

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    [Fact]
    public async Task Login_ValidCredentials_Returns200WithToken()
    {
        var email = $"login_{Guid.NewGuid()}@test.com";
        await _client.PostAsJsonAsync("/api/auth/register",
            new RegisterDto { Email = email, Password = "Integration@123" });

        var response = await _client.PostAsJsonAsync("/api/auth/login",
            new LoginDto { Email = email, Password = "Integration@123" });

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var result = await response.Content.ReadFromJsonAsync<AuthResponseDto>();
        Assert.NotEmpty(result!.Token);
    }

    [Fact]
    public async Task Login_WrongPassword_Returns403()
    {
        var email = $"wrong_{Guid.NewGuid()}@test.com";
        await _client.PostAsJsonAsync("/api/auth/register",
            new RegisterDto { Email = email, Password = "Integration@123" });

        var response = await _client.PostAsJsonAsync("/api/auth/login",
            new LoginDto { Email = email, Password = "WrongPassword!" });

        Assert.Equal(HttpStatusCode.Forbidden, response.StatusCode);
    }

    [Fact]
    public async Task GetProjects_WithoutToken_Returns401()
    {
        var response = await _client.GetAsync("/api/projects");

        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }
}
