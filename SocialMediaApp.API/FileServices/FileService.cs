using SocialMediaApp.Core.ServicesContract;

namespace SocialMediaApp.API.FileServices
{
    public class FileService : IFileServices
    {
        private readonly IWebHostEnvironment _environment;

        public FileService(IWebHostEnvironment environment)
        {
            _environment = environment;
        }
        public async Task<string> CreateFile(IFormFile file)
        {
            string newFileName = Guid.NewGuid().ToString() + Path.GetExtension(file.FileName);
            string newPath = Path.Combine(_environment.WebRootPath, "Upload", newFileName);

            using (var stream = new FileStream(newPath, FileMode.Create))
            {
                await file.CopyToAsync(stream);
            }

            return newFileName;
        }
        public async Task DeleteFile(string? imageUrl)
        {
            if (string.IsNullOrEmpty(imageUrl))
            {
                return;
            }

            string imagePath = Path.Combine(_environment.WebRootPath, "Upload", imageUrl);
            if (System.IO.File.Exists(imagePath))
            {
                System.IO.File.Delete(imagePath);
            }
        }
        public async Task<string> UpdateFile(IFormFile newFile, string? currentFileName)
        {
            await DeleteFile(currentFileName);

            return await CreateFile(newFile);
        }
    }
}
