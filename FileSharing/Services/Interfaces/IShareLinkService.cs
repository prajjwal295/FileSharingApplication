using FileSharing.DTOs.ShareLinks;

namespace FileSharing.Services.Interfaces
{

    public interface IShareLinkService
    {
        Task<ShareLinkResponse?> CreateAsync(Guid fileId, CreateShareLinkRequest request);
        Task<ShareFileDownloadResponse?> DownloadAsync(string token, string? password);
    }
}
