using FileSharing.Services.Interfaces;

namespace FileSharing.Services
{
    public class MockVirusScanner : IVirusScanner
    {
        public Task<bool> IsSafeAsync(string filePath)
        {
            return Task.FromResult(true);
        }
    }
}
