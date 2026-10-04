using FileSharing.Data;
using FileSharing.DTOs.Auth;
using FileSharing.Exceptions;
using FileSharing.Models;
using FileSharing.Models.Enums;
using FileSharing.Services.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace FileSharing.Services
{
    public class AuthService : IAuthService
    {
        private readonly AppDbContext _db;
        private readonly IJwtService _jwtService;

        public AuthService(AppDbContext db, IJwtService jwtService)
        {
            _db = db;
            _jwtService = jwtService;
        }

        public async Task RegisterAsync(RegisterRequest request)
        {
            var exists = await _db.Users.AnyAsync(x => x.Email == request.Email);

            if (exists)
                throw new BusinessException(
                    "Email already exists");

            var user = new User
            {
                Id = Guid.NewGuid(),
                Name = request.Name,
                Email = request.Email,
                CreatedAt = DateTime.UtcNow
            };

            var auth = new UserAuthentication
            {
                Id = Guid.NewGuid(),
                UserId = user.Id,
                Provider = AuthenticationProviderEnum.Local,
                PasswordHash =
                    BCrypt.Net.BCrypt.HashPassword(
                        request.Password),
                CreatedAt = DateTime.UtcNow
            };

            _db.Users.Add(user);
            _db.UserAuthentications.Add(auth);
            await _db.SaveChangesAsync();
        }

        public async Task<LoginResponse> LoginAsync(LoginRequest request)
        {
            var user = await _db.Users
                .Include(x => x.Authentications)
                .FirstOrDefaultAsync(
                    x => x.Email == request.Email);

            if (user == null)
                throw new BusinessException(
                    "Invalid credentials");

            var auth = user.Authentications
                .FirstOrDefault(x =>
                    x.Provider ==
                    AuthenticationProviderEnum.Local);

            if (auth == null)
                throw new BusinessException(
                    "Local login not configured");

            var validPassword = BCrypt.Net.BCrypt.Verify(
                    request.Password,
                    auth.PasswordHash);

            if (!validPassword)
                throw new BusinessException(
                    "Invalid credentials");

            user.LastLoginAt = DateTime.UtcNow;

            await _db.SaveChangesAsync();

            return new LoginResponse
            {
                Token = _jwtService.GenerateToken(user),
                Name = user.Name,
                Email = user.Email
            };
        }
    }
}
