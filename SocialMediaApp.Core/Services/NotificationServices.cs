using AutoMapper;
using Microsoft.Extensions.Logging;
using SocialMediaApp.Core.Domain.Entites;
using SocialMediaApp.Core.DTO.NotificationDTO;
using SocialMediaApp.Core.IUnitOfWorkConfig;
using SocialMediaApp.Core.RepositoriesContract;
using SocialMediaApp.Core.ServicesContract;
using System.Linq.Expressions;

namespace SocialMediaApp.Core.Services
{
    public class NotificationServices : INotificationServices
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly ILogger<NotificationServices> _logger;
        private readonly INotificationRepository _notificationRepository;
        private readonly IMapper _mapper;

        public NotificationServices(
            IUnitOfWork unitOfWork,
            ILogger<NotificationServices> logger,
            INotificationRepository notificationRepository,
            IMapper mapper)
        {
            _unitOfWork = unitOfWork;
            _logger = logger;
            _notificationRepository = notificationRepository;
            _mapper = mapper;
        }

        private async Task ExecuteWithTransaction(Func<Task> action)
        {
            using (var transaction = await _unitOfWork.BeginTransactionAsync())
            {
                try
                {
                    await action();
                    await _unitOfWork.CommitTransactionAsync();
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, ex.Message);
                    await _unitOfWork.RollbackTransactionAsync();
                    throw;
                }
            }
        }
        public async Task MarkAsReadAsync(Guid notificationID)
        {
            var notification = await _unitOfWork.Repository<Notification>().GetByAsync(n => n.NotificationID == notificationID);
            if (notification != null && !notification.IsRead)
            {
                notification.IsRead = true;
                await _notificationRepository.UpdateAsync(notification);
                await _unitOfWork.CompleteAsync();
            }
        }

        public async Task<int> GetUnreadNotificationCount(Guid profileID)
        {
            return await _unitOfWork.Repository<Notification>()
                .CountAsync(n => n.ProfileID == profileID && !n.IsRead);
        }
        public async Task<NotificationResponse> CreateNotification(NotificationAddRequest notificationAddRequest)
        {
            var notification = _mapper.Map<Notification>(notificationAddRequest);
            notification.NotificationID = Guid.NewGuid();
            await _unitOfWork.Repository<Notification>().CreateAsync(notification);
            await _unitOfWork.CompleteAsync();
            return _mapper.Map<NotificationResponse>(notification);
        }
        public async Task<bool> DeleteAsync(Guid? notificationID)
        {
            if (notificationID == null)
                throw new ArgumentNullException(nameof(notificationID));

            var notification = await _notificationRepository
                .GetByAsync(x => x.NotificationID == notificationID);

            if (notification == null)
                throw new KeyNotFoundException("Notification not found");

            await ExecuteWithTransaction(async () =>
            {
                await _unitOfWork.Repository<Notification>().DeleteAsync(notification);
            });

            return true;
        }

        public async Task<IEnumerable<NotificationResponse>> GetAllAsync(Expression<Func<Notification, bool>>? predicate, int pageIndex = 1, int pageSize = 10)
        {
            pageIndex = pageIndex <= 0 ? 1 : pageIndex;
            pageSize = pageSize <= 0 ? 10 : pageSize;

            var notifications = await _unitOfWork.Repository<Notification>().GetAllAsync(
                predicate,
                includeProperties: "Profile,Profile.User",
                pageIndex: pageIndex,
                pageSize: pageSize
            );

            return  _mapper.Map<IEnumerable<NotificationResponse>>(notifications);
        }

        public async Task<NotificationResponse> GetByAsync(Expression<Func<Notification, bool>> predicate, bool IsTracked = true)
        {
            var notification = await _unitOfWork.Repository<Notification>().GetByAsync(
                predicate,
                IsTracked,
                includeProperties: "Profile,Profile.User"
            );

            return _mapper.Map<NotificationResponse>(notification);
        }
    }
}
