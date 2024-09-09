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
    public class RetweetRepository : GenericRepository<Retweet>, IRetweetRepository
    {
        public RetweetRepository(ApplicationDbContext db) : base(db)
        {
        }
    }
}
