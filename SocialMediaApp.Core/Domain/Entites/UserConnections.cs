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
        public ICollection<ConnectionProfile> ConnectionProfiles { get; set; } = new List<ConnectionProfile>();
        public ICollection<Profile> Profiles { get; set; } = new List<Profile>();
    }
}
