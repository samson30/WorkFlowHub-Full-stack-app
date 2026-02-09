using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using WorkFlowHub.Api.DTOs;
using WorkFlowHub.Api.Models;
using WorkFlowHub.Api.Services;

namespace WorkFlowHub.Api.Controllers
{
    [ApiController]
    [Route("api/projects/{projectId}/tasks")]
    [Authorize]
    public class TasksController : ControllerBase
    {
        private readonly ITaskService _taskService;

        public TasksController(ITaskService taskService)
        {
            _taskService = taskService;
        }

        private int GetUserId()
        {
            return int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)!.Value);
        }

    [HttpGet]
public async Task<IActionResult> GetTasks(int projectId)
{
    var tasks = await _taskService.GetTasksForProjectAsync(projectId, GetUserId());

    var response = tasks.Select(t => new TaskResponseDto
    {
        Id = t.Id,
        Title = t.Title,
        Description = t.Description,
        Status = t.Status,
        CreatedAt = t.CreatedAt,
        ProjectId = t.ProjectId
    });

    return Ok(response);
}


     [HttpPost]
public async Task<IActionResult> CreateTask(int projectId, CreateTaskDto dto)
{
    var task = new TaskItem
    {
        Title = dto.Title,
        Description = dto.Description,
        Status = dto.Status,
        ProjectId = projectId
    };

    var created = await _taskService.CreateTaskAsync(task, GetUserId());

    if (created == null) return NotFound("Project not found");
    return Ok(new TaskResponseDto
{
    Id = created.Id,
    Title = created.Title,
    Description = created.Description,
    Status = created.Status,
    CreatedAt = created.CreatedAt,
    ProjectId = created.ProjectId
});

}


       [HttpPut("{taskId}")]
public async Task<IActionResult> UpdateTask(int taskId, UpdateTaskDto dto)
{
    
    var updatedTask = new TaskItem
    {
        Title = dto.Title,
        Description = dto.Description,
        Status = dto.Status
    };

    var success = await _taskService.UpdateTaskAsync(taskId, GetUserId(), updatedTask);
    if (!success) return NotFound();

    return NoContent();
}
[HttpPatch("{taskId}/status")]
public async Task<IActionResult> UpdateTaskStatus(int projectId, int taskId, [FromBody] UpdateStatusDto dto)
{
    var success = await _taskService.UpdateTaskStatusAsync(taskId, GetUserId(), dto.Status);
    if (!success) return NotFound();
    return NoContent();
}



        [HttpDelete("{taskId}")]
        public async Task<IActionResult> DeleteTask(int taskId)
        {
            var success = await _taskService.DeleteTaskAsync(taskId, GetUserId());
            if (!success) return NotFound();
            return NoContent();
        }
    }
}
