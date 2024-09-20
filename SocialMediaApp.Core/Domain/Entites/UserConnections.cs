using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SocialMediaApp.Core.Domain.Entites
{
    public class UserConnections
    {
        public Guid UserConnectionID { get; set; }
        public Guid FollowerID { get; set; }
        public Guid FollowedID { get; set; }
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public Profile Profile { get; set; } 
        public Guid ProfileId { get; set; }
    }
}
