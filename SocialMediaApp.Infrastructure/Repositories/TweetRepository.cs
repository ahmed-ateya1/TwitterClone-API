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
    public class TweetRepository : GenericRepository<Tweet>, ITweetRepositroy
    {
        public TweetRepository(ApplicationDbContext db) : base(db)
        {
        }
    }
}
