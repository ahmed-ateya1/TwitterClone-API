using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SocialMediaApp.Core.DTO.CommentDTO
{
    public class CommentAddRequest
    {
        [Required(ErrorMessage = "Content is required")]
        [StringLength(280, MinimumLength = 1, ErrorMessage = "Content must be between 1 and 280 characters")]
        public string Content { get; set; }
        [Required(ErrorMessage = "TweetID is required")]
        public Guid TweetID { get; set; }
        public Guid? ParentID { get; set; }
        public IEnumerable<IFormFile>? FormFiles { get; set; } = new List<IFormFile>(); 
    }
}
