using System.Security.Claims;
using FileSharing.Services.Interfaces;

namespace FileSharing.Services;

public class CurrentUserService : ICurrentUserService
{
    private readonly IHttpContextAccessor _httpContextAccessor;

    public CurrentUserService(IHttpContextAccessor httpContextAccessor)
    {
        _httpContextAccessor = httpContextAccessor;
    }

    public Guid UserId
    {
        get
        {
            var value = _httpContextAccessor
                .HttpContext?
                .User
                .FindFirst(ClaimTypes.NameIdentifier)?
                .Value;

            return Guid.Parse(value!);
        }
    }

    public string Email =>
        _httpContextAccessor
            .HttpContext?
            .User
            .FindFirst(ClaimTypes.Email)?
            .Value ?? string.Empty;

    public string Name =>
        _httpContextAccessor
            .HttpContext?
            .User
            .FindFirst(ClaimTypes.Name)?
            .Value ?? string.Empty;
}