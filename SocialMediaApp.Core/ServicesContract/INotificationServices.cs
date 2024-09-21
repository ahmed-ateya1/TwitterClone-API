using SocialMediaApp.Core.Domain.Entites;
using SocialMediaApp.Core.DTO.NotificationDTO;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;

namespace SocialMediaApp.Core.ServicesContract
{
    public interface INotificationServices
    {
        Task<int> GetUnreadNotificationCount(Guid profileID);
        Task<NotificationResponse> CreateNotification(NotificationAddRequest notificationAddRequest);
        Task MarkAsReadAsync(Guid? notificationID);
        Task MarkAllAsReadAsync(Guid? profileID);
        Task<bool> DeleteAsync(Guid? notificationID);
        Task<NotificationResponse> GetByAsync(Expression<Func<Notification, bool>> predicate , bool IsTracked = true);
        Task<IEnumerable<NotificationResponse>> GetAllAsync(Expression<Func<Notification, bool>>? predicate,int pageSize=1 , int pageIndex=10);

    }
}
