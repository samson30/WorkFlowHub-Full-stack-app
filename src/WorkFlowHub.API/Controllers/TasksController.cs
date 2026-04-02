using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using WorkFlowHub.Core.DTOs.Common;
using WorkFlowHub.Core.DTOs.Tasks;
using WorkFlowHub.Core.Interfaces.Services;

namespace WorkFlowHub.API.Controllers;

[ApiController]
[Route("api/projects/{projectId:guid}/tasks")]
[Authorize]
[Produces("application/json")]
public class TasksController : ControllerBase
{
    private readonly ITaskService _taskService;

    public TasksController(ITaskService taskService)
    {
        _taskService = taskService;
    }

    private Guid CurrentUserId =>
        Guid.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);

    [HttpGet]
    [ProducesResponseType(typeof(PagedResult<TaskResponseDto>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetTasks(Guid projectId, [FromQuery] PaginationParams pagination)
    {
        var result = await _taskService.GetTasksAsync(projectId, CurrentUserId, pagination);
        return Ok(result);
    }

    [HttpGet("{taskId:guid}")]
    [ProducesResponseType(typeof(TaskResponseDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetTask(Guid projectId, Guid taskId)
    {
        var task = await _taskService.GetTaskByIdAsync(projectId, taskId, CurrentUserId);
        if (task == null) return NotFound();
        return Ok(task);
    }

    [HttpPost]
    [ProducesResponseType(typeof(TaskResponseDto), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> CreateTask(Guid projectId, [FromBody] CreateTaskDto dto)
    {
        var task = await _taskService.CreateTaskAsync(projectId, dto, CurrentUserId);
        return CreatedAtAction(nameof(GetTask), new { projectId, taskId = task.Id }, task);
    }

    [HttpPut("{taskId:guid}")]
    [ProducesResponseType(typeof(TaskResponseDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> UpdateTask(Guid projectId, Guid taskId, [FromBody] UpdateTaskDto dto)
    {
        var task = await _taskService.UpdateTaskAsync(projectId, taskId, dto, CurrentUserId);
        return Ok(task);
    }

    [HttpPatch("{taskId:guid}/status")]
    [ProducesResponseType(typeof(TaskResponseDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> UpdateTaskStatus(Guid projectId, Guid taskId, [FromBody] UpdateStatusDto dto)
    {
        var task = await _taskService.UpdateTaskStatusAsync(projectId, taskId, dto, CurrentUserId);
        return Ok(task);
    }

    [HttpDelete("{taskId:guid}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> DeleteTask(Guid projectId, Guid taskId)
    {
        await _taskService.DeleteTaskAsync(projectId, taskId, CurrentUserId);
        return NoContent();
    }
}
