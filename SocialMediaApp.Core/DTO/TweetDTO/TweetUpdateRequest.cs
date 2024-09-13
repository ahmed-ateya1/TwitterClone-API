using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SocialMediaApp.Core.DTO.TweetDTO
{
    public class TweetUpdateRequest
    {
        [Required(ErrorMessage = "TweetID is required")]
        public Guid TweetID { get; set; }
        [Required(ErrorMessage = "Content is required")]
        [StringLength(280, ErrorMessage = "Content must be less than 280 characters")]
        public string Content { get; set; }
        [Required(ErrorMessage = "Genre is required")]
        public Guid GenreID { get; set; }
        public List<IFormFile>? TweetFiles { get; set; }
    }
}
