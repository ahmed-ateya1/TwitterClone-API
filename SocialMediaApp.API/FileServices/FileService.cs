using SocialMediaApp.Core.ServicesContract;

namespace SocialMediaApp.API.FileServices
{
    /// <summary>
    /// Provides file-related services for the application.
    /// </summary>
    public class FileService : IFileServices
    {
        private readonly IWebHostEnvironment _environment;
        private readonly IHttpContextAccessor _httpContextAccessor;

        /// <summary>
        /// Initializes a new instance of the <see cref="FileService"/> class.
        /// </summary>
        /// <param name="environment">The web host environment.</param>
        /// <param name="httpContextAccessor">The HTTP context accessor.</param>
        public FileService(IWebHostEnvironment environment, IHttpContextAccessor httpContextAccessor)
        {
            _environment = environment;
            _httpContextAccessor = httpContextAccessor;
        }

        /// <summary>
        /// Creates a new file in the "Upload" directory.
        /// </summary>
        /// <param name="file">The file to be created.</param>
        /// <returns>The URL of the created file.</returns>
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
            var baseUrl = GetBaseUrl() + "Upload/" + newFileName;

            return baseUrl;
        }

        /// <summary>
        /// Deletes a file from the "Upload" directory.
        /// </summary>
        /// <param name="imageUrl">The URL of the file to be deleted.</param>
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

        /// <summary>
        /// Updates a file in the "Upload" directory.
        /// </summary>
        /// <param name="newFile">The new file to be updated.</param>
        /// <param name="currentFileName">The URL of the current file to be replaced.</param>
        /// <returns>The URL of the updated file.</returns>
        public async Task<string> UpdateFile(IFormFile newFile, string? currentFileName)
        {
            await DeleteFile(currentFileName).ConfigureAwait(false);

            return await CreateFile(newFile).ConfigureAwait(false);
        }

        private string GetBaseUrl()
        {
            var request = _httpContextAccessor.HttpContext.Request;
            return $"{request.Scheme}://{request.Host.Value}/";
        }
    }
}
