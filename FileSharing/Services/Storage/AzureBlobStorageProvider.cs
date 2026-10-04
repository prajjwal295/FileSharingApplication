using Azure.Storage.Blobs;
using Azure.Storage.Blobs.Models;
using Azure.Storage.Blobs.Specialized;
using FileSharing.Services.Interfaces;

namespace FileSharing.Services.Storage;

public class AzureBlobStorageProvider : IStorageProvider
{
    private readonly BlobContainerClient _container;
    private readonly ILogger<AzureBlobStorageProvider> _logger;

    public AzureBlobStorageProvider(BlobContainerClient container, ILogger<AzureBlobStorageProvider> logger)
    {
        _container = container;
        _logger = logger;
    }

    public async Task<string> UploadAsync(IFormFile file, CancellationToken cancellationToken = default)
    {
        var extension = Path.GetExtension(file.FileName);

        var storedFileName = $"{Guid.NewGuid()}{extension}";

        var blobClient = _container.GetBlobClient(storedFileName);

        await using var stream = file.OpenReadStream();

        var options = new BlobUploadOptions
        {
            HttpHeaders = new BlobHttpHeaders
            {
                ContentType = file.ContentType
            }
        };

        await blobClient.UploadAsync(
            stream,
            options,
            cancellationToken);

        _logger.LogInformation( "File uploaded to Azure Blob Storage as {BlobName}",storedFileName);

        return storedFileName;
    }

    public async Task<Stream?> DownloadAsync(string storagePath, CancellationToken cancellationToken = default)
    {
        var blobClient =  _container.GetBlobClient(storagePath);

        var exists = await blobClient.ExistsAsync(cancellationToken);

        if (!exists.Value)
        {
            return null;
        }

        var response = await blobClient.DownloadStreamingAsync(cancellationToken: cancellationToken);

        return response.Value.Content;
    }

    public async Task<bool> DeleteAsync(string storagePath, CancellationToken cancellationToken = default)
    {
        var blobClient = _container.GetBlobClient(storagePath);

        var response = await blobClient.DeleteIfExistsAsync(
                DeleteSnapshotsOption.IncludeSnapshots,
                cancellationToken: cancellationToken);

        if (response.Value)
        {
            _logger.LogInformation(
                "Blob {BlobName} deleted from Azure",
                storagePath);
        }

        return response.Value;
    }

    public async Task<bool> ExistsAsync(string storagePath, CancellationToken cancellationToken = default)
    {
        var blobClient = _container.GetBlobClient(storagePath);

        var response = await blobClient.ExistsAsync(cancellationToken);

        return response.Value;
    }

    public async Task<string> UploadAsync(Stream stream, string storagePath, CancellationToken cancellationToken = default)
    {
        var blobClient = _container.GetBlobClient(storagePath);
        stream.Position = 0;

        await blobClient.UploadAsync(
            stream,
            overwrite: true,
            cancellationToken);

        _logger.LogInformation(
            "Chunk uploaded to Azure Blob Storage as {BlobName}",
            storagePath);

        return storagePath;
    }

    public async Task<string> CombineAsync(IEnumerable<string> sourcePaths, string destinationPath, CancellationToken cancellationToken = default)
    {
        try
        {
            var destinationBlob = _container.GetBlockBlobClient(destinationPath);
            var blockIds = new List<string>();
            var chunkNumber = 1;

            foreach (var sourcePath in sourcePaths)
            {
                var sourceBlob = _container.GetBlobClient(sourcePath);

                if (!await sourceBlob.ExistsAsync(cancellationToken))
                {
                    throw new FileNotFoundException(
                        $"Chunk not found: {sourcePath}");
                }

                // Azure block IDs must be Base64 encoded.
                var blockId = Convert.ToBase64String(System.Text.Encoding.UTF8.GetBytes(chunkNumber.ToString("D8")));
                await destinationBlob.StageBlockFromUriAsync(sourceBlob.Uri, blockId,cancellationToken: cancellationToken);
                blockIds.Add(blockId);
                chunkNumber++;
            }

            await destinationBlob.CommitBlockListAsync(blockIds,cancellationToken: cancellationToken);
            _logger.LogInformation(
                "Combined {ChunkCount} chunks into Azure blob {BlobName}",
                blockIds.Count,
                destinationPath);

            return destinationPath;
        }
        catch (Exception ex)
        {
            _logger.LogError(
                ex,
                "Failed to combine chunks into Azure blob {BlobName}",
                destinationPath);

            throw;
        }
    }
}