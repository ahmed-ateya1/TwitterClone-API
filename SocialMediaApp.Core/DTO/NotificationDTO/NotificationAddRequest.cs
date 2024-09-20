using SocialMediaApp.Core.Enumeration;
using System.ComponentModel.DataAnnotations;

namespace SocialMediaApp.Core.DTO.NotificationDTO
{
    public class NotificationAddRequest
    {
        [Required(ErrorMessage = "Message Can't be blank.")]
        [StringLength(250, ErrorMessage = "Message can't be more than 250 characters.")]
        public string? Message { get; set; }

        [Required(ErrorMessage = "Notification Type Can't be blank.")]
        public NotificationType NotificationType { get; set; }

        public string? ReferenceURL { get; set; }

        [Required(ErrorMessage = "Profile ID Can't be blank.")]
        public Guid ProfileID { get; set; }
    }
}
