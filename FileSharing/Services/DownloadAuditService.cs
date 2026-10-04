using FileSharing.Data;
using FileSharing.Enums;
using FileSharing.Models;
using FileSharing.Services.Interfaces;

namespace FileSharing.Services
{
    public class DownloadAuditService: IDownloadAuditService
    {
        private readonly AppDbContext _db;
        private readonly IRequestInfoService _requestInfo;

        public DownloadAuditService(AppDbContext db, IRequestInfoService requestInfo)
        {
            _db = db;
            _requestInfo = requestInfo;
        }

        public async Task LogDownloadAsync(Guid fileId, Guid? shareLinkId)
        {
            var audit = new DownloadAudit
            {
                Id = Guid.NewGuid(),
                FileId = fileId,
                ShareLinkId = shareLinkId.Value,
                IpAddress = _requestInfo.GetIpAddress(),
                DownloadedAtUtc = DateTime.UtcNow
            };

            await _db.DownloadAudits.AddAsync(audit);

            await _db.SaveChangesAsync();
        }
    }
}
