namespace FileSharing.Models
{
    public class DownloadAudit
    {
        public Guid Id { get; set; }

        public Guid FileId { get; set; }

        public Guid ShareLinkId { get; set; }

        public DateTime DownloadedAtUtc { get; set; }

        public bool IsSuccessful { get; set; }

        public string? FailureReason { get; set; }

        public string? IpAddress { get; set; }

        public virtual FileEntity File { get; set; } = null!;

        public virtual ShareLink? ShareLink { get; set; }
    }
}
