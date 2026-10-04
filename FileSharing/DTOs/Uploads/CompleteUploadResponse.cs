namespace FileSharing.DTOs.Uploads
{
    public class CompleteUploadResponse
    {
        public Guid FileId { get; set; }

        public string FileName { get; set; } = string.Empty;
    }
}
