using FileSharing.Options;
using FileSharing.Services.Interfaces;
using Microsoft.Extensions.Options;

namespace FileSharing.Services
{
    public class FileValidationService : IFileValidationService
    {
        private readonly FileUploadOptions _options;

        public FileValidationService(IOptions<FileUploadOptions> options)
        {
            _options = options.Value;
        }

        public void Validate(IFormFile file)
        {
            if (file == null || file.Length == 0)
            {
                throw new ArgumentException(
                    "File is empty.");
            }

            var maxBytes = _options.MaxFileSizeMb * 1024 * 1024;

            if (file.Length > maxBytes)
            {
                throw new ArgumentException("File exceeds maximum size.");
            }

            var extension = Path.GetExtension(file.FileName)
                    .ToLowerInvariant();

            if (!_options.AllowedExtensions
                .Contains(extension))
            {
                throw new ArgumentException(
                    "File type not allowed.");
            }
        }
    }
}
