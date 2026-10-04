namespace FileSharing.Models
{
    public class User
    {
        public Guid Id { get; set; }

        public string Email { get; set; } = string.Empty;

        public string Name { get; set; } = string.Empty;

        public bool IsActive { get; set; } = true;

        public DateTime CreatedAt { get; set; }

        public DateTime? LastLoginAt { get; set; }

        public ICollection<UserAuthentication> Authentications { get; set; }
            = new List<UserAuthentication>();
        public ICollection<FileEntity> Files { get; set; }
            = new List<FileEntity>();
    }
}
