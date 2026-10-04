using FileSharing.Data;
using FileSharing.DTOs.Uploads;
using FileSharing.Models;
using FileSharing.Models.Enums;
using FileSharing.Services.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace FileSharing.Services
{
    public class UploadService : IUploadService
    {
        private readonly AppDbContext _db;
        private readonly ICurrentUserService _currentUser;
        private readonly ILogger<UploadService> _logger;
        private readonly IStorageProvider _storageProvider;
        private readonly IHashService _hashService;

        private const int ChunkSize = 5 * 1024 * 1024; // 5 MB

        public UploadService(AppDbContext db, ICurrentUserService currentUser, ILogger<UploadService> logger, IStorageProvider storageProvider, IHashService hashService)
        {
            _db = db;
            _currentUser = currentUser;
            _logger = logger;
            _storageProvider = storageProvider;
            _hashService = hashService;
        }

        public async Task<InitiateUploadResponse> InitiateUploadAsync(InitiateUploadRequest request)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(request.FileName))
                {
                    throw new ArgumentException("File name is required.");
                }

                if (request.FileSize <= 0)
                {
                    throw new ArgumentException("File size must be greater than zero.");
                }

                var totalChunks = (int)Math.Ceiling((double)request.FileSize / ChunkSize);

                var uploadSession = new FileUploadSession
                {
                    Id = Guid.NewGuid(),
                    OwnerId = _currentUser.UserId,
                    OriginalFileName = request.FileName,
                    TotalSize = request.FileSize,
                    MimeType = request.ContentType,
                    ChunkSize = ChunkSize,
                    TotalChunks = totalChunks,
                    Status = UploadStatus.Pending,
                    CreatedAt = DateTime.UtcNow,
                    ExpiresAt = DateTime.UtcNow.AddHours(2)
                };

                _db.FileUploadSessions.Add(uploadSession);

                await _db.SaveChangesAsync();

                _logger.LogInformation(
                    "Upload session {UploadSessionId} created for user {UserId}. Total chunks: {TotalChunks}",
                    uploadSession.Id,
                    _currentUser.UserId,
                    totalChunks);

                return new InitiateUploadResponse
                {
                    UploadSessionId = uploadSession.Id,
                    ChunkSize = ChunkSize,
                    TotalChunks = totalChunks,
                    ExpiresAt = uploadSession.ExpiresAt
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(
                    ex,
                    "Failed to initiate upload for user {UserId}",
                    _currentUser.UserId);

                throw;
            }
        }

        public async Task UploadChunkAsync(
                    Guid uploadSessionId,
                    int chunkNumber,
                    Stream chunkStream,
                    long chunkSize,
                    CancellationToken cancellationToken = default)
        {
            try
            {
                var userId = _currentUser.UserId;

                var session = await _db.FileUploadSessions
                    .FirstOrDefaultAsync(x =>
                        x.Id == uploadSessionId &&
                        x.OwnerId == userId);

                if (session == null)
                {
                    throw new KeyNotFoundException(
                        "Upload session not found.");
                }

                if (session.Status == UploadStatus.Completed)
                {
                    throw new InvalidOperationException(
                        "Upload is already completed.");
                }

                if (session.ExpiresAt <= DateTime.UtcNow)
                {
                    session.Status = UploadStatus.Expired;

                    await _db.SaveChangesAsync();

                    throw new InvalidOperationException(
                        "Upload session has expired.");
                }

                if (chunkNumber < 1 ||
                    chunkNumber > session.TotalChunks)
                {
                    throw new ArgumentException(
                        "Invalid chunk number.");
                }

                // Calculate expected size.
                var isLastChunk =
                    chunkNumber == session.TotalChunks;

                var expectedChunkSize = isLastChunk
                    ? session.TotalSize -
                      ((long)(session.TotalChunks - 1) *
                       session.ChunkSize)
                    : session.ChunkSize;

                if (chunkSize != expectedChunkSize)
                {
                    throw new ArgumentException(
                        $"Invalid chunk size. Expected {expectedChunkSize} bytes.");
                }

                // Check if this chunk was already uploaded.
                var existingChunk = await _db.FileChunks
                    .FirstOrDefaultAsync(x =>
                        x.UploadSessionId == uploadSessionId &&
                        x.ChunkNumber == chunkNumber);

                if (existingChunk != null)
                {
                    _logger.LogInformation(
                        "Chunk {ChunkNumber} already exists for upload {UploadSessionId}",
                        chunkNumber,
                        uploadSessionId);

                    return;
                }

                await using var memoryStream = new MemoryStream();
                await chunkStream.CopyToAsync(memoryStream, cancellationToken);
                memoryStream.Position = 0;
                var hash = await _hashService.ComputeHashAsync(memoryStream, cancellationToken);
                memoryStream.Position = 0;
                var storagePath = $"chunks/{uploadSessionId}/{chunkNumber}.chunk";

                await _storageProvider.UploadAsync(
                    memoryStream,
                    storagePath,
                    cancellationToken);

                var chunk = new FileChunk
                {
                    Id = Guid.NewGuid(),
                    UploadSessionId = uploadSessionId,
                    ChunkNumber = chunkNumber,
                    Size = chunkSize,
                    Hash = hash,
                    StoragePath = storagePath,
                    CreatedAt = DateTime.UtcNow
                };

                _db.FileChunks.Add(chunk);
                session.Status = UploadStatus.Uploading;
                await _db.SaveChangesAsync(cancellationToken);

                _logger.LogInformation(
                    "Chunk {ChunkNumber}/{TotalChunks} uploaded for session {UploadSessionId}",
                    chunkNumber,
                    session.TotalChunks,
                    uploadSessionId);
            }
            catch (Exception ex)
            {
                _logger.LogError(
                    ex,
                    "Failed to upload chunk {ChunkNumber} for session {UploadSessionId}",
                    chunkNumber,
                    uploadSessionId);

                throw;
            }
        }

        public async Task<CompleteUploadResponse> CompleteUploadAsync(
            Guid uploadSessionId,
            CancellationToken cancellationToken = default)
        {
            try
            {
                var userId = _currentUser.UserId;

                var session = await _db.FileUploadSessions
                    .Include(x => x.Chunks)
                    .FirstOrDefaultAsync(
                        x => x.Id == uploadSessionId &&
                             x.OwnerId == userId,
                        cancellationToken);

                if (session == null)
                {
                    throw new KeyNotFoundException(
                        "Upload session not found.");
                }

                if (session.Status == UploadStatus.Completed)
                {
                    throw new InvalidOperationException(
                        "Upload is already completed.");
                }

                if (session.ExpiresAt <= DateTime.UtcNow)
                {
                    session.Status = UploadStatus.Expired;

                    await _db.SaveChangesAsync(cancellationToken);

                    throw new InvalidOperationException(
                        "Upload session has expired.");
                }

                // ---------------------------------------
                // Validate chunk count
                // ---------------------------------------

                if (session.Chunks.Count != session.TotalChunks)
                {
                    throw new InvalidOperationException(
                        $"Upload is incomplete. " +
                        $"Expected {session.TotalChunks} chunks, " +
                        $"but received {session.Chunks.Count}.");
                }

                // ---------------------------------------
                // Validate chunk numbers
                // ---------------------------------------

                var uploadedChunkNumbers = session.Chunks
                    .Select(x => x.ChunkNumber)
                    .OrderBy(x => x)
                    .ToList();

                var expectedChunkNumbers = Enumerable
                    .Range(1, session.TotalChunks)
                    .ToList();

                if (!uploadedChunkNumbers.SequenceEqual(
                        expectedChunkNumbers))
                {
                    throw new InvalidOperationException(
                        "One or more chunks are missing.");
                }

                // ---------------------------------------
                // Validate total size
                // ---------------------------------------

                var uploadedSize = session.Chunks
                    .Sum(x => x.Size);

                if (uploadedSize != session.TotalSize)
                {
                    throw new InvalidOperationException(
                        $"Uploaded size does not match expected size. " +
                        $"Expected: {session.TotalSize}, " +
                        $"Actual: {uploadedSize}.");
                }

                // ---------------------------------------
                // Combine chunks
                // ---------------------------------------

                var extension =
                    Path.GetExtension(session.OriginalFileName);

                var storedFileName =
                    $"{Guid.NewGuid()}{extension}";

                var finalStoragePath =
                    $"files/{storedFileName}";

                var chunkPaths = session.Chunks
                    .OrderBy(x => x.ChunkNumber)
                    .Select(x => x.StoragePath)
                    .ToList();

                await _storageProvider.CombineAsync(
                    chunkPaths,
                    finalStoragePath,
                    cancellationToken);

                // ---------------------------------------
                // Create FileEntity
                // ---------------------------------------

                var file = new FileEntity
                {
                    Id = Guid.NewGuid(),
                    OwnerId = userId,
                    OriginalFileName = session.OriginalFileName,
                    StoredFileName = finalStoragePath,
                    FileSize = session.TotalSize,
                    MimeType = session.MimeType,
                    CreatedAt = DateTime.UtcNow
                };

                _db.Files.Add(file);

                // ---------------------------------------
                // Complete session
                // ---------------------------------------

                session.Status = UploadStatus.Completed;
                session.FileId = file.Id;

                await _db.SaveChangesAsync(cancellationToken);

                _logger.LogInformation(
                    "Upload {UploadSessionId} completed successfully. File {FileId} created.",
                    uploadSessionId,
                    file.Id);

                return new CompleteUploadResponse
                {
                    FileId = file.Id,
                    FileName = file.OriginalFileName
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(
                    ex,
                    "Failed to complete upload {UploadSessionId}",
                    uploadSessionId);

                throw;
            }
        }

        public async Task<UploadStatusResponse> GetUploadStatusAsync(Guid uploadSessionId, CancellationToken cancellationToken = default)
        {
            try
            {
                var userId = _currentUser.UserId;

                var session = await _db.FileUploadSessions
                    .Include(x => x.Chunks)
                    .FirstOrDefaultAsync(
                        x => x.Id == uploadSessionId &&
                             x.OwnerId == userId,
                        cancellationToken);

                if (session == null)
                {
                    throw new KeyNotFoundException(
                        "Upload session not found.");
                }

                var uploadedChunks = session.Chunks
                    .Select(x => x.ChunkNumber)
                    .OrderBy(x => x)
                    .ToList();

                var uploadedSet = uploadedChunks.ToHashSet();

                var missingChunks = Enumerable
                    .Range(1, session.TotalChunks)
                    .Where(x => !uploadedSet.Contains(x))
                    .ToList();

                return new UploadStatusResponse
                {
                    UploadSessionId = session.Id,
                    Status = session.Status.ToString(),
                    TotalChunks = session.TotalChunks,
                    UploadedChunks = uploadedChunks,
                    MissingChunks = missingChunks
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(
                    ex,
                    "Failed to get upload status for session {UploadSessionId}",
                    uploadSessionId);

                throw;
            }
        }
    }
}
