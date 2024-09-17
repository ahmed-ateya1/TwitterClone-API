using SocialMediaApp.Core.Domain.Entites;
using SocialMediaApp.Core.DTO.FilesCommentDTO;
using SocialMediaApp.Core.DTO.FilesTweetDTO;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SocialMediaApp.Core.ServicesContract
{
    public interface ICommentFilesServices 
    {
        Task<IEnumerable<CommentFiles>> SaveTweetFileAsync(FilesCommentAddRequest? fileCommentAdd);
        Task<bool> DeleteTweetFileAsync(IEnumerable<CommentFiles> files);
    }
}
