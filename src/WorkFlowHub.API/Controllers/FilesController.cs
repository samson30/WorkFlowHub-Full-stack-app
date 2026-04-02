using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using WorkFlowHub.Core.DTOs.Files;
using WorkFlowHub.Core.Interfaces.Services;

namespace WorkFlowHub.API.Controllers;

[ApiController]
[Route("api/files")]
[Authorize]
[Produces("application/json")]
public class FilesController : ControllerBase
{
    private readonly IFileService _fileService;

    public FilesController(IFileService fileService)
    {
        _fileService = fileService;
    }

    private Guid CurrentUserId =>
        Guid.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);

    [HttpPost("upload")]
    [ProducesResponseType(typeof(FileResponseDto), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [RequestSizeLimit(50_000_000)]
    public async Task<IActionResult> Upload(IFormFile file)
    {
        if (file == null || file.Length == 0)
            return BadRequest("No file provided.");

        var result = await _fileService.UploadFileAsync(file, CurrentUserId);
        return CreatedAtAction(nameof(Download), new { fileName = result.FileName }, result);
    }

    [HttpGet("{fileName}")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Download(string fileName)
    {
        var (stream, contentType, name) = await _fileService.DownloadFileAsync(fileName);
        return File(stream, contentType, name);
    }
}
