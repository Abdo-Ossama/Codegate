using CodegateTest.Services.IServices;

namespace CodegateTest.Services
{
    public class ImageService : IImageService
    {
        private const long MaxImageSize = 5 * 1024 * 1024;
        private readonly string _webRootPath;
        private readonly ILogger<ImageService> _logger;

        public ImageService(IWebHostEnvironment environment, ILogger<ImageService> logger)
        {
            _webRootPath = environment.WebRootPath ??
                Path.Combine(environment.ContentRootPath, "wwwroot");
            _logger = logger;
        }

        private string GetFolderPath(string folderName)
        {
            if (folderName is not ("courses_img" or "instructors_img" or "profiles"))
                throw new ArgumentException("Invalid image folder.");

            return Path.Combine(_webRootPath, "img", folderName);
        }

        public string? GetImageUrl(string? fileName, string folderName)
        {
            if (string.IsNullOrWhiteSpace(fileName))
                return null;

            if (Uri.TryCreate(fileName, UriKind.Absolute, out var uri) &&
                (uri.Scheme == Uri.UriSchemeHttps || uri.Scheme == Uri.UriSchemeHttp))
                return fileName;

            GetFolderPath(folderName);
            if (fileName != Path.GetFileName(fileName) ||
                fileName.Contains('/') || fileName.Contains('\\'))
                return null;

            return $"/img/{folderName}/{Uri.EscapeDataString(fileName)}";
        }
        private readonly string[] _allowedExtensions =
        {
        ".png",
        ".jpg",
        ".jpeg"
    };

        public async Task<string> UploadImageAsync(
            IFormFile image,
            string folderName
        )
        {
            if (image.Length == 0)
                throw new ArgumentException("The image file is empty.");
            if (image.Length > MaxImageSize)
                throw new ArgumentException("The image must not exceed 5 MB.");

            var folderPath = GetFolderPath(folderName);
            var extension =
                Path.GetExtension(image.FileName)
                .ToLowerInvariant();

            if (!_allowedExtensions.Contains(extension))
            {
                throw new ArgumentException(
                    "Only PNG and JPG images are allowed"
                );
            }

            var newFile =
                Guid.NewGuid().ToString("N") +
                DateTime.UtcNow.ToString("yyyy-MM-dd") +
                extension;

            var filePath = Path.Combine(folderPath, newFile);

            Directory.CreateDirectory(
                Path.GetDirectoryName(filePath)!
            );

            try
            {
                await using var stream = new FileStream(filePath, FileMode.CreateNew);
                await image.CopyToAsync(stream);
            }
            catch
            {
                DeleteImage(newFile, folderName);
                throw;
            }

            return newFile;
        }

        public bool DeleteImage(
      string fileName,
      string folderName
  )
        {
            if (string.IsNullOrWhiteSpace(fileName) ||
                fileName != Path.GetFileName(fileName) ||
                fileName.Contains('/') || fileName.Contains('\\'))
            {
                return false;
            }

            var oldPhotoPath = Path.Combine(GetFolderPath(folderName), fileName);
            try
            {
                if (!File.Exists(oldPhotoPath))
                    return false;

                File.Delete(oldPhotoPath);
                return true;
            }
            catch (Exception exception) when (
                exception is IOException or UnauthorizedAccessException)
            {
                _logger.LogWarning(exception, "Failed to delete image {FileName}", fileName);
            }

            return false;
        }
    }
}
