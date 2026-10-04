using FileSharing.Models.Enums;

namespace FileSharing.Models
{
    public class FileUploadSession
    {
        public Guid Id { get; set; }

        public Guid OwnerId { get; set; }

        public string OriginalFileName { get; set; } = string.Empty;

        public long TotalSize { get; set; }

        public string MimeType { get; set; } = string.Empty;

        public int ChunkSize { get; set; }

        public int TotalChunks { get; set; }

        public UploadStatus Status { get; set; }

        public DateTime CreatedAt { get; set; }

        public DateTime ExpiresAt { get; set; }

        // Created only after upload is completed
        public Guid? FileId { get; set; }

        public FileEntity? File { get; set; }

        public ICollection<FileChunk> Chunks { get; set; }
            = new List<FileChunk>();
    }
}