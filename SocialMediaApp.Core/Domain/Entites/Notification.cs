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
        public string Title { get; set; }
        public string Content { get; set; }
        public DateTime CreatedAt { get; set; }
        public Guid ProfileID { get; set; }
        public Profile Profile { get; set; }
    }
}
