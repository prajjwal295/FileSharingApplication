using FileSharing.DTOs.Auth;
using FileSharing.Exceptions;
using FileSharing.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;
using System.Security.Claims;

namespace FileSharing.Controllers;

[ApiController]
[Route("api/[controller]")]
public class AuthController : ControllerBase
{
    private readonly IAuthService _authService;
    private readonly ICurrentUserService _currentUser;

    public AuthController(IAuthService authService,ICurrentUserService currentUser)
    {
        _authService = authService;
        _currentUser = currentUser;
    }

    [HttpPost("register")]
    public async Task<IActionResult> Register(RegisterRequest request)
    {
        try
        {
            await _authService.RegisterAsync(request);
            return Ok();
        }
        catch (BusinessException ex)
        {
            return BadRequest(ex.Message);
        }
    }

    [EnableRateLimiting("login-policy")]
    [HttpPost("login")]
    public async Task<IActionResult> Login(LoginRequest request)
    {
        try
        {
            var response =await _authService.LoginAsync(request);
            return Ok(response);
        }
        catch (BusinessException ex)
        {
            return Unauthorized(ex.Message);
        }
    }

    [Authorize]
    [HttpGet("me")]
    public IActionResult Me()
    {
        return Ok(new
        {
            _currentUser.UserId,
            _currentUser.Email,
            _currentUser.Name
        });
    }
}
