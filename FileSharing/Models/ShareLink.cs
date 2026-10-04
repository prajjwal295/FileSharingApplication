namespace FileSharing.Models
{
    public class ShareLink
    {
        public Guid Id { get; set; }

        public Guid FileId { get; set; }

        public string Token { get; set; } = string.Empty;

        public string? PasswordHash { get; set; }

        public DateTime? ExpiresAt { get; set; }

        public int? MaxDownloads { get; set; }

        public int DownloadCount { get; set; }

        public bool IsRevoked { get; set; }

        public DateTime CreatedAt { get; set; }

        public FileEntity File { get; set; } = null!;

        public ICollection<DownloadAudit> DownloadAudits { get; set; } = new List<DownloadAudit>();
    }
}
