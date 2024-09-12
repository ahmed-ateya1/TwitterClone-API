using System;

namespace SocialMediaApp.Core.DTO.TweetDTO
{
    public class TweetResponse
    {
        public Guid TweetID { get; set; }
        public string Content { get; set; }
        public long TotalLikes { get; set; }
        public long TotalRetweets { get; set; }
        public long TotalComments { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime UpdatedAt { get; set; }
        public bool IsUpdated
        {
            get
            {
                return UpdatedAt.Date > CreatedAt.Date;
            }
        }
        public Guid ProfileID { get; set; }
        public Guid GenreID { get; set; }
        public string UserName { get; set; }
        public string FullName { get; set; }
        public string ProfilePictureURL { get; set; }
        public string GenreName { get; set; }
        public List<string> FilesURL { get; set; } = new List<string>();
    }
}
