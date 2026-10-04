using FileSharing.DTOs.ShareLinks;
using FileSharing.Services.Interfaces;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;

namespace FileSharing.Controllers;

[ApiController]
[Route("api/share")]
public class PublicShareController : ControllerBase
{
    private readonly IShareLinkService _shareLinkService;

    public PublicShareController(IShareLinkService shareLinkService)
    {
        _shareLinkService = shareLinkService;
    }

    [EnableRateLimiting("share-download-policy")]
    [HttpPost("{token}")]
    public async Task<IActionResult> Download(string token, DownloadShareLinkRequest request)
    {
        var file = await _shareLinkService.DownloadAsync(
                    token,
                    request.Password);

        if (file == null)
        {
            return NotFound();
        }

        return File(
            file.Stream,
            file.ContentType,
            file.FileName);
    }
}