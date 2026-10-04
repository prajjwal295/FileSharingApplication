namespace FileSharing.DTOs.Files
{
    public class FileDownloadResponse
    {
        public Stream Stream { get; set; } = null!;

        public string FileName { get; set; } = string.Empty;

        public string ContentType { get; set; } = string.Empty;
    }
}
