namespace FileSharing.Services.Interfaces
{
    public interface IHashService
    {
        Task<string> ComputeHashAsync(
            Stream stream,
            CancellationToken cancellationToken = default);
    }
}
