namespace FileSharing.DTOs.ShareLinks
{
    public class CreateShareLinkRequest
    {
        public string? Password { get; set; }

        public DateTime? ExpiresAt { get; set; }

        public int? MaxDownloads { get; set; }
    }
}
