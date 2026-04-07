using Microsoft.EntityFrameworkCore;
using WorkFlowHub.Core.DTOs.Common;
using WorkFlowHub.Core.DTOs.Tasks;
using WorkFlowHub.Core.Enums;
using WorkFlowHub.Core.Models;
using WorkFlowHub.Infrastructure.Data;
using WorkFlowHub.Infrastructure.Repositories;
using WorkFlowHub.Infrastructure.Services;
using Xunit;
using TaskStatus = WorkFlowHub.Core.Enums.TaskStatus;

namespace WorkFlowHub.UnitTests.Services;

public class TaskServiceTests : IDisposable
{
    private readonly AppDbContext _context;
    private readonly TaskService _service;
    private readonly Guid _userId    = Guid.NewGuid();
    private readonly Guid _projectId = Guid.NewGuid();

    public TaskServiceTests()
    {
        var options = new DbContextOptionsBuilder<AppDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;
        _context = new AppDbContext(options);
        _service = new TaskService(
            new TaskRepository(_context),
            new ProjectRepository(_context));

        _context.Users.Add(new User { Id = _userId, Email = "owner@test.com", PasswordHash = "hash" });
        _context.Projects.Add(new Project { Id = _projectId, Name = "Test Project", OwnerId = _userId });
        _context.SaveChanges();
    }

    [Fact]
    public async Task CreateTask_ReturnsTaskWithCorrectDefaults()
    {
        var result = await _service.CreateTaskAsync(
            _projectId,
            new CreateTaskDto { Title = "Task 1", Priority = TaskPriority.High },
            _userId);

        Assert.Equal("Task 1", result.Title);
        Assert.Equal(_projectId, result.ProjectId);
        Assert.Equal(TaskPriority.High, result.Priority);
        Assert.Equal(TaskStatus.Todo, result.Status);
    }

    [Fact]
    public async Task GetTasks_ReturnsOnlyTasksForProject()
    {
        await _service.CreateTaskAsync(_projectId, new CreateTaskDto { Title = "Task A" }, _userId);

        var result = await _service.GetTasksAsync(_projectId, _userId, new PaginationParams());

        Assert.Single(result.Items);
        Assert.Equal("Task A", result.Items.First().Title);
    }

    [Fact]
    public async Task GetTasks_PaginationWorks()
    {
        for (int i = 1; i <= 12; i++)
            await _service.CreateTaskAsync(_projectId, new CreateTaskDto { Title = $"Task {i}" }, _userId);

        var page1 = await _service.GetTasksAsync(_projectId, _userId, new PaginationParams { PageNumber = 1, PageSize = 10 });
        var page2 = await _service.GetTasksAsync(_projectId, _userId, new PaginationParams { PageNumber = 2, PageSize = 10 });

        Assert.Equal(10, page1.Items.Count());
        Assert.Equal(2, page2.Items.Count());
        Assert.Equal(12, page1.TotalCount);
    }

    [Fact]
    public async Task UpdateTaskStatus_ChangesStatusCorrectly()
    {
        var task = await _service.CreateTaskAsync(_projectId, new CreateTaskDto { Title = "Task" }, _userId);

        var result = await _service.UpdateTaskStatusAsync(
            _projectId, task.Id,
            new UpdateStatusDto { Status = TaskStatus.InProgress },
            _userId);

        Assert.Equal(TaskStatus.InProgress, result.Status);
    }

    [Fact]
    public async Task UpdateTask_UpdatesAllFields()
    {
        var task = await _service.CreateTaskAsync(_projectId, new CreateTaskDto { Title = "Old" }, _userId);
        var due = DateTime.UtcNow.AddDays(7);

        var result = await _service.UpdateTaskAsync(
            _projectId, task.Id,
            new UpdateTaskDto { Title = "New", Description = "Updated", Priority = TaskPriority.Critical, DueDate = due },
            _userId);

        Assert.Equal("New", result.Title);
        Assert.Equal("Updated", result.Description);
        Assert.Equal(TaskPriority.Critical, result.Priority);
        Assert.NotNull(result.DueDate);
    }

    [Fact]
    public async Task DeleteTask_SoftDeletesAndHidesFromQuery()
    {
        var task = await _service.CreateTaskAsync(_projectId, new CreateTaskDto { Title = "Task" }, _userId);

        await _service.DeleteTaskAsync(_projectId, task.Id, _userId);

        var fetched = await _service.GetTaskByIdAsync(_projectId, task.Id, _userId);
        Assert.Null(fetched);
    }

    [Fact]
    public async Task CreateTask_NonOwner_ThrowsUnauthorizedAccessException()
    {
        await Assert.ThrowsAsync<UnauthorizedAccessException>(
            () => _service.CreateTaskAsync(_projectId, new CreateTaskDto { Title = "Hack" }, Guid.NewGuid()));
    }

    [Fact]
    public async Task UpdateTask_NonOwner_ThrowsUnauthorizedAccessException()
    {
        var task = await _service.CreateTaskAsync(_projectId, new CreateTaskDto { Title = "Task" }, _userId);

        await Assert.ThrowsAsync<UnauthorizedAccessException>(
            () => _service.UpdateTaskAsync(_projectId, task.Id, new UpdateTaskDto { Title = "Hack" }, Guid.NewGuid()));
    }

    [Fact]
    public async Task GetTaskById_WrongProject_ReturnsNull()
    {
        var task = await _service.CreateTaskAsync(_projectId, new CreateTaskDto { Title = "Task" }, _userId);

        var otherProjectId = Guid.NewGuid();
        _context.Projects.Add(new Project { Id = otherProjectId, Name = "Other", OwnerId = _userId });
        await _context.SaveChangesAsync();

        var result = await _service.GetTaskByIdAsync(otherProjectId, task.Id, _userId);
        Assert.Null(result);
    }

    public void Dispose() => _context.Dispose();
}
