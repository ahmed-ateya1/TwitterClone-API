using SocialMediaApp.Core.Domain.Entites;
using SocialMediaApp.Core.DTO.TweetDTO;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;

namespace SocialMediaApp.Core.ServicesContract
{
    public interface ITweetServices
    {
        Task<TweetResponse> CreateAsync(TweetAddRequest? tweetAddRequest);
        Task<TweetResponse> DeleteAsync(Guid? tweetID);
        Task<TweetResponse> GetByAsync(Expression<Func<Tweet, bool>>? filter = null, bool isTracked = true, string includeProperties = "");
        Task<TweetResponse> GetAllAsync(Expression<Func<Tweet, bool>>? filter = null, string includeProperties = "", Expression<Func<Tweet, object>>? orderBy = null, int pageIndex = 1, int pageSize = 10);
    }
}
