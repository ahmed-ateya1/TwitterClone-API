using Microsoft.AspNetCore.Mvc;
using SocialMediaApp.Core.DTO;
using SocialMediaApp.Core.ServicesContract;
using System.Net;

namespace SocialMediaApp.API.Controllers
{
    /// <summary>
    /// Controller to manage notifications for user profiles.
    /// </summary>
    [Route("api/[controller]")]
    [ApiController]
    public class NotificationController : ControllerBase
    {
        private readonly INotificationServices _notificationServices;
        private readonly ILogger<NotificationController> _logger;

        /// <summary>
        /// Initializes a new instance of the <see cref="NotificationController"/> class.
        /// </summary>
        /// <param name="notificationServices">Service to handle notification operations.</param>
        /// <param name="logger">Logger instance for logging information.</param>
        public NotificationController(INotificationServices notificationServices,
            ILogger<NotificationController> logger)
        {
            _notificationServices = notificationServices;
            _logger = logger;
        }

        /// <summary>
        /// Retrieves notifications for a specific user profile.
        /// </summary>
        /// <param name="profileID">The ID of the profile to retrieve notifications for.</param>
        /// <param name="pageIndex">The page index for pagination. Default is 1.</param>
        /// <param name="pageSize">The number of items per page for pagination. Default is 10.</param>
        /// <returns>A list of notifications for the specified profile.</returns>
        /// <response code="200">Returns the list of notifications.</response>
        /// <response code="500">If an error occurs while retrieving notifications.</response>
        [HttpGet("getNotificationsForProfile/{profileID}")]
        public async Task<ActionResult<ApiResponse>> GetNotificationsForProfile(Guid profileID, int pageIndex = 1, int pageSize = 10)
        {
            try
            {
                var notifications = await _notificationServices.GetAllAsync(x => x.ProfileID == profileID, pageIndex, pageSize);
                return Ok(new ApiResponse
                {
                    IsSuccess = true,
                    StatusCode = HttpStatusCode.OK,
                    Messages = "Notifications retrieved successfully",
                    Result = notifications
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error in GetNotificationsForProfile");
                return StatusCode(StatusCodes.Status500InternalServerError,
                    "Error in GetNotificationsForProfile");
            }
        }

        /// <summary>
        /// Retrieves a specific notification by its ID and mark this notification as readed
        /// </summary>
        /// <param name="notificationID">The ID of the notification to retrieve.</param>
        /// <returns>The notification with the specified ID.</returns>
        /// <response code="200">Returns the notification.</response>
        /// <response code="404">If the notification is not found.</response>
        /// <response code="500">If an error occurs while retrieving the notification.</response>
        [HttpGet("getNotification/{notificationID}")]
        public async Task<ActionResult<ApiResponse>> GetNotification(Guid notificationID)
        {
            try
            {
                var notification = await _notificationServices.GetByAsync(x => x.NotificationID == notificationID);
                if (notification == null)
                {
                    return NotFound(new ApiResponse
                    {
                        IsSuccess = false,
                        StatusCode = HttpStatusCode.NotFound,
                        Messages = "Notification not found",
                        Result = null
                    });
                }
                return Ok(new ApiResponse
                {
                    IsSuccess = true,
                    StatusCode = HttpStatusCode.OK,
                    Messages = "Notification retrieved successfully",
                    Result = notification
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error in GetNotification");
                return StatusCode(StatusCodes.Status500InternalServerError,
                    "Error in GetNotification");
            }
        }

        /// <summary>
        /// Deletes a specific notification by its ID.
        /// </summary>
        /// <param name="notificationID">The ID of the notification to delete.</param>
        /// <returns>A response indicating whether the deletion was successful.</returns>
        /// <response code="200">If the notification is deleted successfully.</response>
        /// <response code="404">If the notification is not found.</response>
        /// <response code="500">If an error occurs while deleting the notification.</response>
        [HttpDelete("deleteNotification/{notificationID}")]
        public async Task<ActionResult<ApiResponse>> DeleteNotification(Guid notificationID)
        {
            try
            {
                var isDeleted = await _notificationServices.DeleteAsync(notificationID);
                if (!isDeleted)
                {
                    return NotFound(new ApiResponse
                    {
                        IsSuccess = false,
                        StatusCode = HttpStatusCode.NotFound,
                        Messages = "Notification not found",
                        Result = isDeleted
                    });
                }
                return Ok(new ApiResponse
                {
                    IsSuccess = true,
                    StatusCode = HttpStatusCode.OK,
                    Messages = "Notification deleted successfully",
                    Result = isDeleted
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error in DeleteNotification");
                return StatusCode(StatusCodes.Status500InternalServerError,
                    "Error in DeleteNotification");
            }
        }

        /// <summary>
        /// Marks all notifications as read for a specific user profile.
        /// </summary>
        /// <param name="profileID">The ID of the profile whose notifications will be marked as read.</param>
        /// <returns>A response indicating that all notifications have been marked as read.</returns>
        /// <response code="200">If all notifications are marked as read successfully.</response>
        /// <response code="500">If an error occurs while marking notifications as read.</response>
        [HttpPut("markAllAsRead/{profileID}")]
        public async Task<ActionResult<ApiResponse>> MarkAllAsRead(Guid profileID)
        {
            try
            {
                await _notificationServices.MarkAllAsReadAsync(profileID);
                return Ok(new ApiResponse
                {
                    IsSuccess = true,
                    StatusCode = HttpStatusCode.OK,
                    Messages = "All notifications marked as read successfully",
                    Result = "Success"
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error in MarkAllAsRead");
                return StatusCode(StatusCodes.Status500InternalServerError,
                    "Error in MarkAllAsRead");
            }
        }
    }
}
