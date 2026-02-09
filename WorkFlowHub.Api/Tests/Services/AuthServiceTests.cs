using FluentAssertions;
using Microsoft.Extensions.Configuration;
using WorkFlowHub.Api.DTOs;
using WorkFlowHub.Api.Services;
using WorkFlowHub.Tests.Helpers;

namespace WorkFlowHub.Tests.Services
{
    public class AuthServiceTests
    {
        private readonly IConfiguration _configuration;

        public AuthServiceTests()
        {
            var configValues = new Dictionary<string, string?>
            {
                { "Jwt:SecretKey", "ThisIsAVeryLongSecretKeyForTesting123456" },
                { "Jwt:Issuer", "TestIssuer" },
                { "Jwt:Audience", "TestAudience" }
            };

            _configuration = new ConfigurationBuilder()
                .AddInMemoryCollection(configValues)
                .Build();
        }

        [Fact]
        public async Task RegisterAsync_WithValidData_ShouldCreateUser()
        {
            // Arrange
            var context = TestDbContextFactory.Create();
            var authService = new AuthService(context, _configuration);
            var dto = new RegisterDto { Email = "test@example.com", Password = "Password123" };

            // Act
            var result = await authService.RegisterAsync(dto);

            // Assert
            result.Should().NotBeNull();
            result.Email.Should().Be("test@example.com");
        }

        [Fact]
        public async Task RegisterAsync_WithDuplicateEmail_ShouldThrowException()
        {
            // Arrange
            var context = TestDbContextFactory.Create();
            var authService = new AuthService(context, _configuration);
            var dto = new RegisterDto { Email = "test@example.com", Password = "Password123" };

            await authService.RegisterAsync(dto);

            // Act & Assert
            await Assert.ThrowsAsync<InvalidOperationException>(
                () => authService.RegisterAsync(dto)
            );
        }

        [Fact]
        public async Task LoginAsync_WithValidCredentials_ShouldReturnToken()
        {
            // Arrange
            var context = TestDbContextFactory.Create();
            var authService = new AuthService(context, _configuration);
            var registerDto = new RegisterDto { Email = "test@example.com", Password = "Password123" };
            await authService.RegisterAsync(registerDto);

            var loginDto = new LoginDto { Email = "test@example.com", Password = "Password123" };

            // Act
            var result = await authService.LoginAsync(loginDto);

            // Assert
            result.Should().NotBeNullOrEmpty();
        }

        [Fact]
        public async Task LoginAsync_WithWrongPassword_ShouldReturnNull()
        {
            // Arrange
            var context = TestDbContextFactory.Create();
            var authService = new AuthService(context, _configuration);
            var registerDto = new RegisterDto { Email = "test@example.com", Password = "Password123" };
            await authService.RegisterAsync(registerDto);

            var loginDto = new LoginDto { Email = "test@example.com", Password = "WrongPassword" };

            // Act
            var result = await authService.LoginAsync(loginDto);

            // Assert
            result.Should().BeNull();
        }
    }
}
