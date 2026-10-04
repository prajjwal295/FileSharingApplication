namespace FileSharing.Services.Interfaces
{
    public interface IVirusScanner
    {
        Task<bool> IsSafeAsync(string filePath);
    }
}
