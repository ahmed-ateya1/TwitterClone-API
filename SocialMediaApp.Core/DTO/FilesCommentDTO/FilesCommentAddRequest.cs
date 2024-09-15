using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SocialMediaApp.Core.DTO.FilesCommentDTO
{
    public class FilesCommentAddRequest
    {
        public Guid CommentID { get; set; }
        public List<IFormFile> formFiles { get; set; } = new List<IFormFile>();
    }
}
