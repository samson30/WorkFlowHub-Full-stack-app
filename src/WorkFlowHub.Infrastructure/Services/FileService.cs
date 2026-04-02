using Azure.Storage.Blobs;
using Azure.Storage.Blobs.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using WorkFlowHub.Core.DTOs.Files;
using WorkFlowHub.Core.Interfaces.Repositories;
using WorkFlowHub.Core.Interfaces.Services;
using WorkFlowHub.Core.Models;

namespace WorkFlowHub.Infrastructure.Services;

public class FileService : IFileService
{
    private readonly IFileRepository _fileRepository;
    private readonly BlobServiceClient _blobServiceClient;
    private const string ContainerName = "workflowhub-files";

    public FileService(IFileRepository fileRepository, IConfiguration configuration)
    {
        _fileRepository = fileRepository;
        var connectionString = configuration["Azure:BlobStorage:ConnectionString"]
            ?? "UseDevelopmentStorage=true";
        _blobServiceClient = new BlobServiceClient(connectionString);
    }

    public async Task<FileResponseDto> UploadFileAsync(IFormFile file, Guid userId)
    {
        var containerClient = _blobServiceClient.GetBlobContainerClient(ContainerName);
        await containerClient.CreateIfNotExistsAsync(PublicAccessType.None);

        var uniqueFileName = $"{Guid.NewGuid()}_{file.FileName}";
        var blobClient = containerClient.GetBlobClient(uniqueFileName);

        await using var stream = file.OpenReadStream();
        await blobClient.UploadAsync(stream, new BlobHttpHeaders { ContentType = file.ContentType });

        var record = new FileRecord
        {
            FileName = uniqueFileName,
            BlobUrl = blobClient.Uri.ToString(),
            UploadedBy = userId
        };

        await _fileRepository.AddAsync(record);

        return new FileResponseDto
        {
            Id = record.Id,
            FileName = record.FileName,
            BlobUrl = record.BlobUrl,
            UploadedBy = record.UploadedBy,
            CreatedAt = record.CreatedAt
        };
    }

    public async Task<(Stream stream, string contentType, string fileName)> DownloadFileAsync(string fileName)
    {
        var record = await _fileRepository.GetByFileNameAsync(fileName)
            ?? throw new KeyNotFoundException("File not found.");

        var containerClient = _blobServiceClient.GetBlobContainerClient(ContainerName);
        var blobClient = containerClient.GetBlobClient(fileName);
        var response = await blobClient.DownloadStreamingAsync();

        return (response.Value.Content,
                response.Value.Details.ContentType ?? "application/octet-stream",
                fileName);
    }
}
