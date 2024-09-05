using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SocialMediaApp.Core.Domain.Entites
{
    public class Retweet
    {
        public Guid RetweetID { get; set; }
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime? UpdatedAt { get; set; } = DateTime.UtcNow;
        public Guid ProfileID { get; set; }
        public Profile Profile { get; set; }
        public Guid TweetID { get; set; }
        public Tweet Tweet { get; set; }
        public Guid CommentID { get; set; }
        public Comment Comment { get; set; }
    }
}
