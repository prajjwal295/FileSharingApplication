namespace FileSharing.Services.Interfaces
{
    public interface IDownloadAuditService
    {
        Task LogDownloadAsync(Guid fileId, Guid? shareLinkId);
    }
}
