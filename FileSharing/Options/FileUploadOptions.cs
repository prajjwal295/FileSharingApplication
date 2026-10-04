namespace FileSharing.Options
{
    public class FileUploadOptions
    {
        public int MaxFileSizeMb { get; set; }
        public List<string> AllowedExtensions { get; set; } = new();
    }
}
