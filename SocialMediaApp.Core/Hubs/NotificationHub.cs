using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.SignalR;
using Microsoft.Extensions.Logging;
using SocialMediaApp.Core.Domain.IdentityEntites;
using SocialMediaApp.Core.DTO.NotificationDTO;
using SocialMediaApp.Core.IUnitOfWorkConfig;
using SocialMediaApp.Core.ServicesContract;
using System.Security.Claims;

public class NotificationHub : Hub
{
    private readonly INotificationServices _notificationServices;
    private readonly IHttpContextAccessor _httpContextAccessor;
    private readonly IUnitOfWork _unitOfWork;
    private readonly UserManager<ApplicationUser> _userManager;

    public NotificationHub(INotificationServices notificationServices,
        IHttpContextAccessor httpContextAccessor,
        IUnitOfWork unitOfWork,
        UserManager<ApplicationUser> userManager)
    {
        _notificationServices = notificationServices;
        _httpContextAccessor = httpContextAccessor;
        _unitOfWork = unitOfWork;
        _userManager = userManager;
    }

    private string GetCurrentUserName()
    {
        var userName = _httpContextAccessor.HttpContext.User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (string.IsNullOrEmpty(userName))
            throw new UnauthorizedAccessException("User is not authenticated");

        return userName;
    }
    private async Task<SocialMediaApp.Core.Domain.Entites.Profile?> GetProfileIfAvailable()
    {
        var userName = GetCurrentUserName();
        if (userName == null) return null;

        var user = await _userManager.FindByNameAsync(userName);
        if (user == null) return null;

        return await _unitOfWork.Repository<SocialMediaApp.Core.Domain.Entites.Profile>().GetByAsync(x => x.User.Id == user.Id);
    }
   
    public async Task SendTotalUnReadNotification()
    {
        var profile = await GetProfileIfAvailable();
        if (profile == null) return;
        var totalNotification =
            await _notificationServices.GetUnreadNotificationCount(profile.ProfileID);

        await Clients.User(profile.UserID.ToString()).SendAsync("ReceiveTotalUnReadNotification", totalNotification);
    }
}
