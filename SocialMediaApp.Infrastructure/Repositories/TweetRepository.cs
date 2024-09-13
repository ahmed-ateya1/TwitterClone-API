using AutoMapper;
using SocialMediaApp.Core.Domain.Entites;
using SocialMediaApp.Core.RepositoriesContract;
using SocialMediaApp.Infrastructure.Data;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static System.Runtime.InteropServices.JavaScript.JSType;

namespace SocialMediaApp.Infrastructure.Repositories
{
    public class TweetRepository : GenericRepository<Tweet>, ITweetRepositroy
    {
        private readonly ApplicationDbContext _db;
        public TweetRepository(ApplicationDbContext db) : base(db)
        {
            _db = db;
        }

        public async Task<Tweet> UpdateAsync(Tweet tweet)
        {
            var tweetToUpdate = _db.Tweets.FirstOrDefault(x => x.TweetID == tweet.TweetID);
            if (tweetToUpdate == null)
                throw new InvalidOperationException("Tweet not found");


            _db.Entry(tweetToUpdate).CurrentValues.SetValues(tweetToUpdate);
            tweetToUpdate.UpdatedAt = DateTime.UtcNow;
            tweetToUpdate.IsUpdated = true;

            await SaveAsync();

            return tweetToUpdate;
        }
    }
}
