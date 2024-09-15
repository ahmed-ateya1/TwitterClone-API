using SocialMediaApp.Core.Domain.Entites;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SocialMediaApp.Core.DTO.CommentDTO
{
    public class CommentResponse
    {
        public Guid CommentID { get; set; }
        public string Content { get; set; }
        public long TotalLikes { get; set; }
        public long TotalRetweet { get; set; }
        public long TotalComment { get; set; }
        public bool IsUpdated { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime UpdatedAt { get; set; }
        public string UserName { get; set; }
        public string FullName { get; set; }
        public string ProfileImageUrl { get; set; }
        public List<CommentResponse> Replies { get; set; } = new List<CommentResponse>();
        public List<string> FilesUrl { get; set; } = new List<string>();
    }
}
