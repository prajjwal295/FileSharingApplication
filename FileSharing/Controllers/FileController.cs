using FileSharing.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace FileSharing.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class FilesController : ControllerBase
    {
        private readonly IFileService _fileService;

        public FilesController(IFileService fileService)
        {
            _fileService = fileService;
        }

        [HttpPost("upload")]
        [Consumes("multipart/form-data")]
        public async Task<IActionResult> Upload(IFormFile file)
        {
            await _fileService.UploadAsync(file);
            return Ok(new
            {
                message = "File uploaded successfully"
            });
        }

        [HttpGet]
        public async Task<IActionResult> GetMyFiles()
        {
            var files = await _fileService.GetMyFilesAsync();

            return Ok(files);
        }

        [HttpGet("{fileId:guid}/download")]
        public async Task<IActionResult> Download(Guid fileId)
        {
            var file = await _fileService.DownloadAsync(fileId);

            if (file == null)
            {
                return NotFound();
            }

            return File(
                file.Stream,
                file.ContentType,
                file.FileName);
        }

        [HttpDelete("{fileId:guid}")]
        public async Task<IActionResult> Delete(Guid fileId)
        {
            var deleted =
                await _fileService.DeleteAsync(fileId);

            if (!deleted)
            {
                return NotFound();
            }

            return Ok(new
            {
                message = "File deleted successfully"
            });
        }
    }
}
