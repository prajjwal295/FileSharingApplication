using FileSharing.Models;
using Microsoft.EntityFrameworkCore;

namespace FileSharing.Data
{
    public class AppDbContext : DbContext
    {
        public AppDbContext(DbContextOptions<AppDbContext> options) : base(options)
        {
        }

        public DbSet<User> Users => Set<User>();
        public DbSet<ShareLink> ShareLinks => Set<ShareLink>();
        public DbSet<UserAuthentication> UserAuthentications => Set<UserAuthentication>();
        public DbSet<FileEntity> Files => Set<FileEntity>();
        public DbSet<DownloadAudit> DownloadAudits => Set<DownloadAudit>();
        public DbSet<FileUploadSession> FileUploadSessions => Set<FileUploadSession>();
        public DbSet<FileChunk> FileChunks => Set<FileChunk>();

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            base.OnConfiguring(optionsBuilder);
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<UserAuthentication>()
                .HasOne(x => x.User)
                .WithMany(x => x.Authentications)
                .HasForeignKey(x => x.UserId);

            modelBuilder.Entity<FileEntity>()
                .HasOne(x => x.Owner)
                .WithMany(x => x.Files)
                .HasForeignKey(x => x.OwnerId);

            modelBuilder.Entity<ShareLink>()
                .HasOne(x => x.File)
                .WithMany()
                .HasForeignKey(x => x.FileId)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<DownloadAudit>()
                .HasOne(x => x.File)
                .WithMany(x => x.DownloadAudits)
                .HasForeignKey(x => x.FileId)
                .OnDelete(DeleteBehavior.NoAction);

            modelBuilder.Entity<DownloadAudit>()
                .HasOne(x => x.ShareLink)
                .WithMany(x => x.DownloadAudits)
                .HasForeignKey(x => x.ShareLinkId)
                .OnDelete(DeleteBehavior.NoAction);

            modelBuilder.Entity<ShareLink>()
                .HasIndex(x => x.Token)
                .IsUnique();

            modelBuilder.Entity<DownloadAudit>()
                        .HasIndex(x => x.FileId);

            modelBuilder.Entity<DownloadAudit>()
                        .HasIndex(x => x.DownloadedAtUtc);

            modelBuilder.Entity<DownloadAudit>()
                        .HasIndex(x => x.ShareLinkId);

            modelBuilder.Entity<FileChunk>()
                        .HasIndex(x => new
                        {
                            x.UploadSessionId,
                            x.ChunkNumber
                        })
                        .IsUnique();

            modelBuilder.Entity<FileChunk>()
                .HasOne(x => x.UploadSession)
                .WithMany(x => x.Chunks)
                .HasForeignKey(x => x.UploadSessionId)
                .OnDelete(DeleteBehavior.Cascade);

            base.OnModelCreating(modelBuilder);
        }

    }
}
