using Microsoft.AspNetCore.Http;
using WorkFlowHub.Core.DTOs.Files;

namespace WorkFlowHub.Core.Interfaces.Services;

public interface IFileService
{
    Task<FileResponseDto> UploadFileAsync(IFormFile file, Guid userId);
    Task<(Stream stream, string contentType, string fileName)> DownloadFileAsync(string fileName);
}
