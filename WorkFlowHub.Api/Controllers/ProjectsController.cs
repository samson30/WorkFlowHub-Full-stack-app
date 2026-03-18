using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using WorkFlowHub.Api.DTOs;
using WorkFlowHub.Api.DTOs.Common;
using WorkFlowHub.Api.DTOs.Projects;
using WorkFlowHub.Api.Models;
using WorkFlowHub.Api.Services;

namespace WorkFlowHub.Api.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class ProjectsController : ControllerBase
    {
        private readonly IProjectService _projectService;

        public ProjectsController(IProjectService projectService)
        {
            _projectService = projectService;
        }

        private int GetUserId()
        {
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            return int.Parse(userIdClaim!);
        }

        [HttpGet]
        public async Task<IActionResult> GetMyProjects([FromQuery] PaginationParams paging)
        {
            int userId = GetUserId();
            var result = await _projectService.GetProjectsForUserAsync(userId, paging);

            return Ok(new PagedResult<ProjectResponseDto>
            {
                Items = result.Items.Select(p => new ProjectResponseDto
                {
                    Id = p.Id,
                    Name = p.Name,
                    Description = p.Description ?? "",
                    Status = p.Status,
                    CreatedAt = p.CreatedAt
                }).ToList(),
                TotalCount = result.TotalCount,
                Page = result.Page,
                PageSize = result.PageSize
            });
        }


        [HttpGet("{id}")]
        public async Task<IActionResult> GetProject(int id)
        {
            int userId = GetUserId();
            var project = await _projectService.GetProjectByIdAsync(id, userId);
            return project == null ? NotFound() : Ok(project);
        }

        [HttpPost]
        public async Task<IActionResult> CreateProject(CreateProjectDto dto)
        {
            int userId = GetUserId();

            var project = new Project
            {
                Name = dto.Name,
                Description = dto.Description,
                Status = dto.Status,
                UserId = userId,
                CreatedAt = DateTime.UtcNow
            };

            var created = await _projectService.CreateProjectAsync(project);
            return Ok(created);
        }


        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateProject(int id, UpdateProjectDto dto)
        {
            int userId = GetUserId();

            var updatedProject = new Project
            {
                Name = dto.Name,
                Description = dto.Description,
                Status = dto.Status
            };

            var success = await _projectService.UpdateProjectAsync(id, userId, updatedProject);
            if (!success) return NotFound();

            return NoContent();
        }



        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteProject(int id)
        {
            int userId = GetUserId();
            var success = await _projectService.DeleteProjectAsync(id, userId);

            if (!success) return NotFound();
            return NoContent();
        }

    }
}
