namespace CodegateTest.Services.IServices
{
    public interface IImageService
    {
        string? GetImageUrl(string? fileName, string folderName);
        
        Task<string> UploadImageAsync(
            IFormFile image,
            string folderName
        );

        bool DeleteImage(
            string fileName,
            string folderName
        );
    }
}

