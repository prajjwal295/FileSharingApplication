using FileSharing.Services.Interfaces;

namespace FileSharing.Services
{
    public class RequestInfoService : IRequestInfoService
    {
        private readonly IHttpContextAccessor _httpContextAccessor;

        public RequestInfoService(IHttpContextAccessor httpContextAccessor)
        {
            _httpContextAccessor = httpContextAccessor;
        }

        public string GetIpAddress()
        {
            return _httpContextAccessor
                .HttpContext?
                .Connection?
                .RemoteIpAddress?
                .ToString()
                ?? "Unknown";
        }
    }
}
