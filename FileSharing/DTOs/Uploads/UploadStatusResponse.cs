namespace FileSharing.DTOs.Uploads
{
    public class UploadStatusResponse
    {
        public Guid UploadSessionId { get; set; }

        public string Status { get; set; } = string.Empty;

        public int TotalChunks { get; set; }

        public List<int> UploadedChunks { get; set; } = new();

        public List<int> MissingChunks { get; set; } = new();
    }
}