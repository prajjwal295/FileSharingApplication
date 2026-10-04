namespace FileSharing.Services.Interfaces
{
    public interface IStorageProvider
    {
        Task<string> UploadAsync(IFormFile file, CancellationToken cancellationToken = default);

        Task<string> UploadAsync(Stream stream, string storagePath, CancellationToken cancellationToken = default);

        Task<Stream?> DownloadAsync(string storagePath,CancellationToken cancellationToken = default);

        Task<bool> DeleteAsync(string storagePath, CancellationToken cancellationToken = default);

        Task<bool> ExistsAsync(string storagePath, CancellationToken cancellationToken = default);

        Task<string> CombineAsync(IEnumerable<string> sourcePaths, string destinationPath, CancellationToken cancellationToken = default);
    }
}
