using System.Security.Cryptography;
using FileSharing.Services.Interfaces;

namespace FileSharing.Services
{
    public class HashService : IHashService
    {
        public async Task<string> ComputeHashAsync(
            Stream stream,
            CancellationToken cancellationToken = default)
        {
            using var sha256 = SHA256.Create();
            var hash = await sha256.ComputeHashAsync(
                stream,
                cancellationToken);

            return Convert.ToHexString(hash).ToLowerInvariant();
        }
    }
}