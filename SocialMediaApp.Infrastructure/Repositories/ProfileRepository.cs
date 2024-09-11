using SocialMediaApp.Core.Domain.Entites;
using SocialMediaApp.Core.RepositoriesContract;
using SocialMediaApp.Infrastructure.Data;

namespace SocialMediaApp.Infrastructure.Repositories
{
    public class ProfileRepository : GenericRepository<Profile>, IProfileRepository
    {
        private readonly ApplicationDbContext _db;

        public ProfileRepository(ApplicationDbContext db) : base(db)
        {
            _db = db;
        }

        public async Task<Profile> UpdateAsync(Profile profile)
        {
            var profileToUpdate = await _db.Profiles.FindAsync(profile.ProfileID);
            if (profileToUpdate == null)
            {
                return null;
            }

            _db.Entry(profileToUpdate).CurrentValues.SetValues(profile);
            profileToUpdate.UpdatedAt = DateTime.UtcNow;

            await SaveAsync();
            return profileToUpdate;
        }
    }
}
