using FileSharing.DTOs.ShareLinks;
using FileSharing.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace FileSharing.Controllers
{
    [ApiController]
    [Route("api/share-links")]
    [Authorize]
    public class ShareLinksController : ControllerBase
    {
        private readonly IShareLinkService _shareLinkService;

        public ShareLinksController(IShareLinkService shareLinkService)
        {
            _shareLinkService = shareLinkService;
        }

        [HttpPost("{fileId:guid}")]
        public async Task<IActionResult> Create(Guid fileId, CreateShareLinkRequest request)
        {
            var result = await _shareLinkService.CreateAsync(fileId, request);
            if (result == null)
            {
                return NotFound();
            }

            return Ok(result);
        }
    }
}
