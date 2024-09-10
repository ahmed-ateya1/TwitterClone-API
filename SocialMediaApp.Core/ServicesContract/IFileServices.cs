using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SocialMediaApp.Core.ServicesContract
{
    public interface IFileServices
    {
        /// <summary>
        /// Creates a new file and saves it to the specified location.
        /// </summary>
        /// <param name="file">The file to be created.</param>
        /// <returns>The name of the newly created file.</returns>
        Task<string> CreateFile(IFormFile file);

        /// <summary>
        /// Deletes the file with the specified image URL.
        /// </summary>
        /// <param name="imageUrl">The URL of the file to be deleted.</param>
        Task DeleteFile(string? imageUrl);

        /// <summary>
        /// Updates the file with a new file and deletes the file with the specified current file name.
        /// </summary>
        /// <param name="newFile">The new file to be updated.</param>
        /// <param name="currentFileName">The name of the current file to be deleted.</param>
        /// <returns>The name of the newly created file.</returns>
        Task<string> UpdateFile(IFormFile newFile, string? currentFileName);
    }
}
