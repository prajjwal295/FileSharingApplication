using FileSharing.Models;

namespace FileSharing.Services.Interfaces
{
    public interface IJwtService
    {
        string GenerateToken(User user);
    }
}
