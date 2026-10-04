using FileSharing.DTOs.Files;

namespace FileSharing.Services.Interfaces
{
    public interface IFileService
    {
        Task UploadAsync(IFormFile file);
        Task<List<FileResponse>> GetMyFilesAsync();
        Task<FileDownloadResponse?> DownloadAsync(Guid fileId);
        Task<bool> DeleteAsync(Guid fileId);

    }
}
