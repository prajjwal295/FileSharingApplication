using FileSharing.Models.Enums;

namespace FileSharing.Models
{
    public class UserAuthentication
    {
        public Guid Id { get; set; }

        public Guid UserId { get; set; }

        public AuthenticationProviderEnum Provider { get; set; }

        public string? ProviderUserId { get; set; }

        public string? PasswordHash { get; set; }

        public DateTime CreatedAt { get; set; }

        public User User { get; set; } = null!;
    }
}
