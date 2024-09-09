using SocialMediaApp.Core.RepositoriesContract;
using SocialMediaApp.Infrastructure.Data;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SocialMediaApp.Infrastructure.Repositories
{
    public class ConnectionProfileRepository : GenericRepository<ConnectionProfile>, IConnectionProfile
    {
        public ConnectionProfileRepository(ApplicationDbContext db) : base(db)
        {
        }
    }
}
