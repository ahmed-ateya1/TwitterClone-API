﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SocialMediaApp.Core.Domain.Entites
{
    public class Tweet
    {
        public Guid TweetID { get; set; }
        public string Content { get; set; }
        public long TotalLikes { get; set; }
        public long TotalRetweets { get; set; }
        public long TotalComments { get; set; }
        public bool IsUpdated { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime UpdatedAt { get; set; }
        public Guid ProfileID { get; set; }
        public Profile Profile { get; set; }
        public Guid? GenreID { get; set; }
        public Genre? Genre { get; set; }
        public Tweet? ParentTweet { get; set; }
        public Guid? ParentTweetID { get; set; }
        public ICollection<Tweet> Retweets { get; set; } = new List<Tweet>();
        public ICollection<TweetFiles> Files { get; set; } = new List<TweetFiles>();
        public ICollection<Comment> Comments { get; set; } = new List<Comment>();
        public ICollection<Like> Likes { get; set; } = new List<Like>();
    }
}
