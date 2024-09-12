using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SocialMediaApp.Core.DTO.FilesTweetDTO
{
    public class FileTweetAddRequest
    {
        public Guid TweetID { get; set; }
        public List<IFormFile> formFiles { get; set; } = new List<IFormFile>();
    }
}
