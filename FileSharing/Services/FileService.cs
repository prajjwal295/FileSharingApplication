using FileSharing.Data;
using FileSharing.DTOs.Files;
using FileSharing.Models;
using FileSharing.Services.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace FileSharing.Services
{
    public class FileService : IFileService
    {
        private readonly AppDbContext _db;
        private readonly ICurrentUserService _currentUser;
        private readonly IFileValidationService _validationService;
        private readonly IStorageProvider _storageProvider;
        private readonly ILogger<FileService> _logger;
        private readonly IDownloadAuditService _auditService;

        public FileService(AppDbContext db, ICurrentUserService currentUser,
             IFileValidationService validationService, IStorageProvider storageProvider, ILogger<FileService> logger, IDownloadAuditService auditService)
        {
            _db = db;
            _currentUser = currentUser;
            _validationService = validationService;
            _storageProvider = storageProvider;
            _logger = logger;
            _auditService = auditService;
        }

        public async Task UploadAsync(IFormFile file)
        {
            if (file == null || file.Length == 0)
            {
                throw new Exception("File is empty.");
            }

            _validationService.Validate(file);

            var storagePath = await _storageProvider.UploadAsync(file);

            var fileEntity = new FileEntity
            {
                Id = Guid.NewGuid(),

                OwnerId = _currentUser.UserId,

                OriginalFileName = file.FileName,

                StoredFileName = storagePath,

                FileSize = file.Length,

                MimeType = file.ContentType,

                CreatedAt = DateTime.UtcNow
            };

            _db.Files.Add(fileEntity);
            await _db.SaveChangesAsync();

            _logger.LogInformation(
                "User {UserId} uploaded file {FileName}",
                _currentUser.UserId,
                file.FileName);
        }

        public async Task<List<FileResponse>> GetMyFilesAsync()
        {
            var userId = _currentUser.UserId;

            return await _db.Files
                .Where(x => x.OwnerId == userId)
                .OrderByDescending(x => x.CreatedAt)
                .Select(x => new FileResponse
                {
                    Id = x.Id,
                    FileName = x.OriginalFileName,
                    FileSize = x.FileSize,
                    MimeType = x.MimeType,
                    CreatedAt = x.CreatedAt
                })
                .ToListAsync();
        }

        public async Task<FileDownloadResponse?> DownloadAsync(Guid fileId)
        {
            var userId = _currentUser.UserId;

            var file = await _db.Files
                .FirstOrDefaultAsync(x =>
                    x.Id == fileId &&
                    x.OwnerId == userId);

            if (file == null)
            {
                return null;
            }

            var stream =
                await _storageProvider.DownloadAsync(
                    file.StoredFileName);

            if (stream == null)
            {
                _logger.LogWarning(
                    "Physical file missing. FileId={FileId}",
                    file.Id);

                return null;
            }
            await _auditService.LogDownloadAsync(file.Id, null);
            return new FileDownloadResponse
            {
                Stream = stream,
                FileName = file.OriginalFileName,
                ContentType = file.MimeType
            };
        }

        public async Task<bool> DeleteAsync(Guid fileId)
        {
            var userId = _currentUser.UserId;

            var file = await _db.Files
                .FirstOrDefaultAsync(x =>
                    x.Id == fileId &&
                    x.OwnerId == userId);

            if (file == null)
            {
                return false;
            }

            await _storageProvider.DeleteAsync(
                file.StoredFileName);

            _db.Files.Remove(file);

            await _db.SaveChangesAsync();

            _logger.LogInformation(
                "User {UserId} deleted file {FileId}",
                userId,
                fileId);

            return true;
        }
    }
}
