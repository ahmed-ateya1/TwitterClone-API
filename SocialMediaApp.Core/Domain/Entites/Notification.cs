using SocialMediaApp.Core.Enumeration;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SocialMediaApp.Core.Domain.Entites
{
    public class Notification
    {
        public Guid NotificationID { get; set; }
        public string Message { get; set; }
        public bool IsRead { get; set; } = false;
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public string NotificationType { get; set; }
        public string? ReferenceURL { get; set; }
        public Guid ProfileID { get; set; }
        public Profile Profile { get; set; }
    }
}
