namespace FileSharing.DTOs.ShareLinks
{
    public class ShareLinkResponse
    {
        public Guid Id { get; set; }

        public string Token { get; set; } = string.Empty;

        public DateTime? ExpiresAt { get; set; }

        public int? MaxDownloads { get; set; }

        public int DownloadCount { get; set; }

        public bool IsRevoked { get; set; }
    }
}
