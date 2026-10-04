using System.Security.Cryptography;
using FileSharing.Data;
using FileSharing.DTOs.ShareLinks;
using FileSharing.Models;
using FileSharing.Services.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace FileSharing.Services;

public class ShareLinkService : IShareLinkService
{
    private readonly AppDbContext _db;
    private readonly ICurrentUserService _currentUser;
    private readonly IDownloadAuditService _auditService;
    private readonly IStorageProvider _storageProvider;

    public ShareLinkService(AppDbContext db, ICurrentUserService currentUser, IDownloadAuditService auditService, IStorageProvider storageProvider)
    {
        _db = db;
        _currentUser = currentUser;
        _auditService = auditService;
        _storageProvider = storageProvider;
    }

    public async Task<ShareLinkResponse?> CreateAsync(Guid fileId, CreateShareLinkRequest request)
    {
        var userId = _currentUser.UserId;

        var file = await _db.Files.FirstOrDefaultAsync(x =>
                x.Id == fileId &&
                x.OwnerId == userId);

        if (file == null)
        {
            return null;
        }

        var token = GenerateToken();

        var shareLink = new ShareLink
        {
            Id = Guid.NewGuid(),
            FileId = fileId,
            Token = token,
            PasswordHash = string.IsNullOrWhiteSpace(request.Password)
                        ? null
                        : BCrypt.Net.BCrypt.HashPassword(
                            request.Password),
            ExpiresAt = request.ExpiresAt,
            MaxDownloads = request.MaxDownloads,
            DownloadCount = 0,
            IsRevoked = false,
            CreatedAt = DateTime.UtcNow
        };

        _db.ShareLinks.Add(shareLink);
        await _db.SaveChangesAsync();

        return new ShareLinkResponse
        {
            Id = shareLink.Id,
            Token = shareLink.Token,
            ExpiresAt = shareLink.ExpiresAt,
            MaxDownloads = shareLink.MaxDownloads,
            DownloadCount = shareLink.DownloadCount,
            IsRevoked = shareLink.IsRevoked
        };
    }

    public async Task<ShareFileDownloadResponse?> DownloadAsync(string token, string? password)
    {
        var shareLink = await _db.ShareLinks
            .Include(x => x.File)
            .FirstOrDefaultAsync(x =>
                x.Token == token);

        if (shareLink == null)
        {
            return null;
        }

        if (shareLink.IsRevoked)
        {
            return null;
        }

        if (shareLink.ExpiresAt.HasValue && shareLink.ExpiresAt.Value < DateTime.UtcNow)
        {
            return null;
        }

        if (shareLink.MaxDownloads.HasValue &&
            shareLink.DownloadCount >=
            shareLink.MaxDownloads.Value)
        {
            return null;
        }

        if (!string.IsNullOrWhiteSpace(shareLink.PasswordHash))
        {
            if (string.IsNullOrWhiteSpace(password))
            {
                return null;
            }

            var valid = BCrypt.Net.BCrypt.Verify(
                    password,
                    shareLink.PasswordHash);

            if (!valid)
            {
                return null;
            }
        }

        var stream = await _storageProvider.DownloadAsync(shareLink.File.StoredFileName);
        if (stream == null)
            return null;
        shareLink.DownloadCount++;
        await _db.SaveChangesAsync();

        try
        {
            await _auditService.LogDownloadAsync(
                shareLink.FileId,
                shareLink.Id);
        }
        catch
        {
        }

        return new ShareFileDownloadResponse
        {
            Stream = stream,
            FileName = shareLink.File.OriginalFileName,
            ContentType = shareLink.File.MimeType
        };
    }


    private static string GenerateToken()
    {
        var bytes = RandomNumberGenerator.GetBytes(32);

        return Convert.ToBase64String(bytes)
            .Replace("+", "-")
            .Replace("/", "_")
            .Replace("=", "");
    }
}