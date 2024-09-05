using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SocialMediaApp.Core.Domain.Entites
{
    public class Comment
    {
        public Guid CommentID { get; set; }
        public long TotalLikes { get; set; }
        public long TotalRetweet { get; set; }
        public long TotalComment { get; set; }
        public bool IsUpdated { get; set; } = false;
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

        public Guid ProfileID { get; set; }
        public Profile Profile { get; set; }

        public Guid TweetID { get; set; }
        public Tweet Tweet { get; set; }

        public Guid? ParentCommentID { get; set; }
        public Comment ParentComment { get; set; }
        public ICollection<Comment> Replies { get; set; } = new List<Comment>();

        public ICollection<Like> Likes { get; set; } = new List<Like>();
        public ICollection<Retweet> Retweets { get; set; } = new List<Retweet>();
    }

}