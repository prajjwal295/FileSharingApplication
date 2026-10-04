namespace FileSharing.DTOs.Uploads
{
    public class InitiateUploadResponse
    {
        public Guid UploadSessionId { get; set; }

        public int ChunkSize { get; set; }

        public int TotalChunks { get; set; }

        public DateTime ExpiresAt { get; set; }
    }
}
