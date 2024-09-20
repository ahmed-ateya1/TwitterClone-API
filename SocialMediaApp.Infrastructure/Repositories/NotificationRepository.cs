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
    public class NotificationRepository : GenericRepository<Notification>, INotificationRepository
    {
        private readonly ApplicationDbContext _db;
        public NotificationRepository(ApplicationDbContext db) : base(db)
        {
            _db = db;
        }

        public async Task<Notification> UpdateAsync(Notification notification)
        {
            var notificationToUpdate = await _db.Notifications.FindAsync(notification.NotificationID);
            if (notificationToUpdate == null)
                return null;
            _db.Entry(notificationToUpdate).CurrentValues.SetValues(notification);
            notificationToUpdate.CreatedAt = DateTime.UtcNow;

            await SaveAsync();
            return notificationToUpdate;
        }
    }
}
