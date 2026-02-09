namespace WorkFlowHub.Api.Interfaces.Services;

public interface IStorageService
{
    Task<string> UploadFileAsync(Stream fileStream, string fileName, string contentType);
    Task<Stream?> DownloadFileAsync(string fileName);
    Task<bool> DeleteFileAsync(string fileName);
}
