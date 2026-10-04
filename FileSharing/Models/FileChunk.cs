namespace FileSharing.Models
{
    public class FileChunk
    {
        public Guid Id { get; set; }

        public Guid UploadSessionId { get; set; }

        public int ChunkNumber { get; set; }

        public long Size { get; set; }

        public string? Hash { get; set; }

        public string StoragePath { get; set; } = string.Empty;

        public DateTime CreatedAt { get; set; }

        public FileUploadSession UploadSession { get; set; }
            = null!;
    }
}
