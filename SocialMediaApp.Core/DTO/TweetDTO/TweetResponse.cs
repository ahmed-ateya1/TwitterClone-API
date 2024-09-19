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
        public bool IsUpdated { get; set; }
        public Guid ProfileID { get; set; }
        public Guid GenreID { get; set; }
        public string UserName { get; set; }
        public string FullName { get; set; }
        public string ProfilePictureURL { get; set; }
        public string? GenreName { get; set; }
        public bool IsLiked { get; set; }
        public List<string> FilesURL { get; set; } = new List<string>();
        //public Guid? ParentTweetID { get; set; }
        //public string? ParentTweetContent { get; set; }
        public TweetResponse? ParentTweet { get; set; }
        public bool IsRetweeted { get; set; }
    }
}