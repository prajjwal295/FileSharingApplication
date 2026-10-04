namespace FileSharing.DTOs.Files
{
    public class FileResponse
    {
        public Guid Id { get; set; }

        public string FileName { get; set; } = string.Empty;

        public long FileSize { get; set; }

        public string MimeType { get; set; } = string.Empty;

        public DateTime CreatedAt { get; set; }
    }
}
