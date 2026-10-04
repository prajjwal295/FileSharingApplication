namespace FileSharing.Services.Interfaces
{
    public interface ICurrentUserService
    {
        Guid UserId { get; }

        string Email { get; }

        string Name { get; }
    }
}
