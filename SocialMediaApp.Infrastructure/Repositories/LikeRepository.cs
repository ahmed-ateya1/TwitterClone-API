using Microsoft.EntityFrameworkCore;
using SocialMediaApp.Core.Domain.Entites;
using SocialMediaApp.Core.RepositoriesContract;
using SocialMediaApp.Infrastructure.Data;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SocialMediaApp.Infrastructure.Repositories
{
    public class LikeRepository : GenericRepository<Like>, ILikeRepository
    {
        private readonly ApplicationDbContext _db;
        public LikeRepository(ApplicationDbContext db) : base(db)
        {
            _db = db;
        }

        public async Task<bool> IsUserLikedToComment(Guid commentID, Guid profileID)
        {
            var like = await _db.Likes
                .FirstOrDefaultAsync(x => x.CommentID == commentID && x.ProfileID == profileID);
            if (like == null)
                return false;
            return true;
        }

        public async Task<bool> IsUserLikedToTweet(Guid tweetID, Guid profileID)
        {
            var like = await _db.Likes
                .FirstOrDefaultAsync(x => x.TweetID == tweetID && x.ProfileID == profileID);
            if (like == null)
                return false;
            return true;
        }

        public async Task<Like> LikeAsync(Like like)
        {
            await _db.Likes.AddAsync(like);
            await _db.SaveChangesAsync();
            return like;
        }

        public async Task<bool> UnLikeAsync(Guid likeID)
        {
            var like = await _db.Likes
                .FirstOrDefaultAsync(x => x.LikeID == likeID);
            if (like == null)
                return false;
            _db.Likes.Remove(like);
            await _db.SaveChangesAsync();
            return true;
        }

    }
}
