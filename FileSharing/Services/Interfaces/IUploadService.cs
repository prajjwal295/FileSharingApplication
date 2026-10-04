using FileSharing.DTOs.Uploads;

namespace FileSharing.Services.Interfaces
{
    public interface IUploadService
    {
        Task<InitiateUploadResponse> InitiateUploadAsync(InitiateUploadRequest request);

        Task UploadChunkAsync(Guid uploadSessionId,int chunkNumber,Stream chunkStream, long chunkSize, CancellationToken cancellationToken = default);

        Task<CompleteUploadResponse> CompleteUploadAsync(Guid uploadSessionId, CancellationToken cancellationToken = default);

        Task<UploadStatusResponse> GetUploadStatusAsync(Guid uploadSessionId, CancellationToken cancellationToken = default);
    }
}
