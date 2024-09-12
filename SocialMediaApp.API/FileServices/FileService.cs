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

            if (!Directory.Exists(Path.Combine(_environment.WebRootPath, "Upload")))
            {
                Directory.CreateDirectory(Path.Combine(_environment.WebRootPath, "Upload"));
            }

            using (var stream = new FileStream(newPath, FileMode.Create))
            {
                await file.CopyToAsync(stream).ConfigureAwait(false);
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
                await Task.Run(() => System.IO.File.Delete(imagePath)).ConfigureAwait(false);
            }
        }

        public async Task<string> UpdateFile(IFormFile newFile, string? currentFileName)
        {
            await DeleteFile(currentFileName).ConfigureAwait(false);

            return await CreateFile(newFile).ConfigureAwait(false);
        }
    }
}
