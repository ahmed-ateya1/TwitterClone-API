using SocialMediaApp.Core.Domain.Entites;
using SocialMediaApp.Core.ServicesContract;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SocialMediaApp.Core.RepositoriesContract
{
    public interface ILikeRepository : IGenericRepository<Like>
    {
        Task<Like> LikeAsync(Like like);
        Task<bool> UnLikeAsync(Guid likeID);
        Task<bool> IsUserLikedToTweet(Guid tweetID, Guid profileID);
        Task<bool> IsUserLikedToComment(Guid commentID, Guid profileID);
    }
}
