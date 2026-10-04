namespace FileSharing.Models
{
    public class FileEntity
    {
        public Guid Id { get; set; }

        public Guid OwnerId { get; set; }

        public string OriginalFileName { get; set; } = string.Empty;

        public string StoredFileName { get; set; } = string.Empty;

        public long FileSize { get; set; }

        public string MimeType { get; set; } = string.Empty;

        public DateTime CreatedAt { get; set; }

        public User Owner { get; set; } = null!;

        public ICollection<DownloadAudit> DownloadAudits { get; set; } = new List<DownloadAudit>();

    }
}
