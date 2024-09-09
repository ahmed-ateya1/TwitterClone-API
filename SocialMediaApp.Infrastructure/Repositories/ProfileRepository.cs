using SocialMediaApp.Core.Domain.Entites;
using SocialMediaApp.Core.RepositoriesContract;
using SocialMediaApp.Infrastructure.Data;

namespace SocialMediaApp.Infrastructure.Repositories
{
    public class ProfileRepository : GenericRepository<Profile> , IProfileRepository
    {
        public ProfileRepository(ApplicationDbContext db) : base(db)
        {
        }
    }
}
