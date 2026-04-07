using Microsoft.EntityFrameworkCore;
using WorkFlowHub.Core.DTOs.Common;
using WorkFlowHub.Core.DTOs.Projects;
using WorkFlowHub.Core.Models;
using WorkFlowHub.Infrastructure.Data;
using WorkFlowHub.Infrastructure.Repositories;
using WorkFlowHub.Infrastructure.Services;
using Xunit;

namespace WorkFlowHub.UnitTests.Services;

public class ProjectServiceTests : IDisposable
{
    private readonly AppDbContext _context;
    private readonly ProjectService _service;
    private readonly Guid _userId = Guid.NewGuid();

    public ProjectServiceTests()
    {
        var options = new DbContextOptionsBuilder<AppDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;
        _context = new AppDbContext(options);
        _service = new ProjectService(new ProjectRepository(_context));

        _context.Users.Add(new User { Id = _userId, Email = "owner@test.com", PasswordHash = "hash" });
        _context.SaveChanges();
    }

    [Fact]
    public async Task CreateProject_ReturnsProjectWithCorrectOwner()
    {
        var result = await _service.CreateProjectAsync(
            new CreateProjectDto { Name = "My Project", Description = "Desc" }, _userId);

        Assert.Equal("My Project", result.Name);
        Assert.Equal(_userId, result.OwnerId);
    }

    [Fact]
    public async Task GetProjects_ReturnsOnlyOwnersProjects()
    {
        var otherId = Guid.NewGuid();
        await _service.CreateProjectAsync(new CreateProjectDto { Name = "Mine" }, _userId);
        await _service.CreateProjectAsync(new CreateProjectDto { Name = "Other" }, otherId);

        var result = await _service.GetProjectsAsync(_userId, new PaginationParams());

        Assert.Single(result.Items);
        Assert.Equal("Mine", result.Items.First().Name);
    }

    [Fact]
    public async Task GetProjects_PaginationReturnsCorrectPage()
    {
        for (int i = 1; i <= 15; i++)
            await _service.CreateProjectAsync(new CreateProjectDto { Name = $"Project {i}" }, _userId);

        var page1 = await _service.GetProjectsAsync(_userId, new PaginationParams { PageNumber = 1, PageSize = 10 });
        var page2 = await _service.GetProjectsAsync(_userId, new PaginationParams { PageNumber = 2, PageSize = 10 });

        Assert.Equal(10, page1.Items.Count());
        Assert.Equal(5, page2.Items.Count());
        Assert.Equal(15, page1.TotalCount);
        Assert.Equal(2, page1.TotalPages);
        Assert.True(page1.HasNextPage);
        Assert.False(page1.HasPreviousPage);
    }

    [Fact]
    public async Task UpdateProject_ByOwner_UpdatesFields()
    {
        var project = await _service.CreateProjectAsync(new CreateProjectDto { Name = "Old" }, _userId);

        var result = await _service.UpdateProjectAsync(
            project.Id, new UpdateProjectDto { Name = "New", Description = "Updated desc" }, _userId);

        Assert.Equal("New", result.Name);
        Assert.Equal("Updated desc", result.Description);
    }

    [Fact]
    public async Task UpdateProject_ByNonOwner_ThrowsUnauthorizedAccessException()
    {
        var project = await _service.CreateProjectAsync(new CreateProjectDto { Name = "Project" }, _userId);

        await Assert.ThrowsAsync<UnauthorizedAccessException>(
            () => _service.UpdateProjectAsync(project.Id, new UpdateProjectDto { Name = "Hacked" }, Guid.NewGuid()));
    }

    [Fact]
    public async Task UpdateProject_NotFound_ThrowsKeyNotFoundException()
    {
        await Assert.ThrowsAsync<KeyNotFoundException>(
            () => _service.UpdateProjectAsync(Guid.NewGuid(), new UpdateProjectDto { Name = "X" }, _userId));
    }

    [Fact]
    public async Task DeleteProject_SoftDeletesAndHidesFromQuery()
    {
        var project = await _service.CreateProjectAsync(new CreateProjectDto { Name = "To Delete" }, _userId);

        await _service.DeleteProjectAsync(project.Id, _userId);

        var fetched = await _service.GetProjectByIdAsync(project.Id, _userId);
        Assert.Null(fetched);
    }

    [Fact]
    public async Task GetProjectById_WrongOwner_ReturnsNull()
    {
        var project = await _service.CreateProjectAsync(new CreateProjectDto { Name = "Project" }, _userId);

        var result = await _service.GetProjectByIdAsync(project.Id, Guid.NewGuid());

        Assert.Null(result);
    }

    public void Dispose() => _context.Dispose();
}
