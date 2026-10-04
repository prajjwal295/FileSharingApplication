using FileSharing.Services.Interfaces;

namespace FileSharing.Services.Storage;

public class LocalStorageProvider : IStorageProvider
{
    private readonly IWebHostEnvironment _environment;
    private readonly ILogger<LocalStorageProvider> _logger;

    public LocalStorageProvider(IWebHostEnvironment environment, ILogger<LocalStorageProvider> logger)
    {
        _environment = environment;
        _logger = logger;
    }

    public async Task<string> UploadAsync(IFormFile file, CancellationToken cancellationToken = default)
    {
        var uploadsFolder = Path.Combine(
                _environment.ContentRootPath,
                "Uploads");

        Directory.CreateDirectory(
            uploadsFolder);

        var extension =
            Path.GetExtension(file.FileName);

        var storedFileName =
            $"{Guid.NewGuid()}{extension}";

        var filePath =
            Path.Combine(
                uploadsFolder,
                storedFileName);

        await using var stream =
            new FileStream(
                filePath,
                FileMode.Create);

        await file.CopyToAsync(
            stream,
            cancellationToken);

        _logger.LogInformation(
            "File stored locally at {Path}",
            filePath);

        return storedFileName;
    }

    public async Task<Stream?> DownloadAsync(string storagePath, CancellationToken cancellationToken = default)
    {
        var uploadsFolder =
            Path.Combine(
                _environment.ContentRootPath,
                "Uploads");

        var filePath =
            Path.Combine(
                uploadsFolder,
                storagePath);

        if (!File.Exists(filePath))
        {
            return null;
        }

        return await Task.FromResult(
            new FileStream(
                filePath,
                FileMode.Open,
                FileAccess.Read,
                FileShare.Read,
                64 * 1024,
                true));
    }

    public async Task<bool> DeleteAsync(string storagePath, CancellationToken cancellationToken = default)
    {
        var uploadsFolder =
            Path.Combine(
                _environment.ContentRootPath,
                "Uploads");

        var filePath =
            Path.Combine(
                uploadsFolder,
                storagePath);

        if (!File.Exists(filePath))
        {
            return false;
        }

        File.Delete(filePath);

        await Task.CompletedTask;

        return true;
    }

    public Task<bool> ExistsAsync(string storagePath, CancellationToken cancellationToken = default)
    {
        var uploadsFolder =
            Path.Combine(
                _environment.ContentRootPath,
                "Uploads");

        var filePath =
            Path.Combine(
                uploadsFolder,
                storagePath);

        return Task.FromResult(File.Exists(filePath));
    }

    public async Task<string> UploadAsync(Stream stream, string storagePath, CancellationToken cancellationToken = default)
    {
        var uploadsFolder = Path.Combine( _environment.ContentRootPath, "Uploads");

        var fullPath = Path.Combine(uploadsFolder,storagePath);

        var directory = Path.GetDirectoryName(fullPath);

        if (!string.IsNullOrEmpty(directory))
        {
            Directory.CreateDirectory(directory);
        }

        await using var fileStream = new FileStream(
            fullPath,
            FileMode.Create,
            FileAccess.Write,
            FileShare.None,
            64 * 1024,
            useAsync: true);

        await stream.CopyToAsync(
            fileStream,
            cancellationToken);

        return storagePath;
    }

    public async Task<string> CombineAsync(IEnumerable<string> sourcePaths,string destinationPath, CancellationToken cancellationToken = default)
    {
        var uploadsFolder = Path.Combine(_environment.ContentRootPath, "Uploads");
        var destinationFullPath = Path.Combine(uploadsFolder,destinationPath);

        var directory = Path.GetDirectoryName(destinationFullPath);

        if (!string.IsNullOrEmpty(directory))
        {
            Directory.CreateDirectory(directory);
        }

        await using var destinationStream = new FileStream(
            destinationFullPath,
            FileMode.Create,
            FileAccess.Write,
            FileShare.None,
            bufferSize: 64 * 1024,
            useAsync: true);

        foreach (var sourcePath in sourcePaths)
        {
            var sourceFullPath = Path.Combine(uploadsFolder,sourcePath);

            if (!File.Exists(sourceFullPath))
            {
                throw new FileNotFoundException( $"Chunk not found: {sourcePath}");
            }

            await using var sourceStream = new FileStream(
                sourceFullPath,
                FileMode.Open,
                FileAccess.Read,
                FileShare.Read,
                bufferSize: 64 * 1024,
                useAsync: true);

            await sourceStream.CopyToAsync(
                destinationStream,
                cancellationToken);
        }

        return destinationPath;
    }
}