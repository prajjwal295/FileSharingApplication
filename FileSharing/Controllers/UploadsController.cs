using FileSharing.DTOs.Uploads;
using FileSharing.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace FileSharing.Controllers
{
    [ApiController]
    [Route("api/uploads")]
    [Authorize]
    public class UploadsController : ControllerBase
    {
        private readonly IUploadService _uploadService;

        public UploadsController(IUploadService uploadService)
        {
            _uploadService = uploadService;
        }

        [HttpPost("initiate")]
        public async Task<IActionResult> InitiateUpload([FromBody] InitiateUploadRequest request)
        {
            var response = await _uploadService.InitiateUploadAsync(request);
            return Ok(response);
        }

        [HttpPost("{uploadSessionId}/chunks/{chunkNumber}")]
        [Consumes("multipart/form-data")]
        public async Task<IActionResult> UploadChunk(Guid uploadSessionId, int chunkNumber, IFormFile chunk, CancellationToken cancellationToken)
        {
            await _uploadService.UploadChunkAsync(
                uploadSessionId,
                chunkNumber,
                chunk.OpenReadStream(),
                chunk.Length,
                cancellationToken);

            return Ok(new
            {
                uploadSessionId,
                chunkNumber,
                message = "Chunk uploaded successfully."
            });
        }

        [HttpPost("{uploadSessionId}/complete")]
        public async Task<IActionResult> CompleteUpload(Guid uploadSessionId, CancellationToken cancellationToken)
        {
            var result = await _uploadService
                    .CompleteUploadAsync(
                        uploadSessionId,
                        cancellationToken);

            return Ok(result);
        }

        [HttpGet("{uploadSessionId}/status")]
        public async Task<IActionResult> GetUploadStatus(Guid uploadSessionId, CancellationToken cancellationToken)
        {
            var result = await _uploadService.GetUploadStatusAsync(
                uploadSessionId,
                cancellationToken);

            return Ok(result);
        }
    }
}