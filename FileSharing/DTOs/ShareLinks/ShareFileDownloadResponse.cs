namespace FileSharing.DTOs.ShareLinks
{
    public class ShareFileDownloadResponse
    {
        public Stream Stream { get; set; } = null!;

        public string FileName { get; set; } = string.Empty;

        public string ContentType { get; set; } = string.Empty;
    }
}
