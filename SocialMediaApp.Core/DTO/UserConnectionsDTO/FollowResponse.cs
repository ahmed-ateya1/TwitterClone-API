using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace SocialMediaApp.Core.DTO.UserConnectionsDTO
{
    public class FollowResponse
    {
        [JsonIgnore]
        public Guid UserConnectionId { get; set; }
        public Guid FollowerID { get; set; }
        public Guid FollowedID { get; set; }
        public DateTime CreatedAt { get; set; } 
    }
}
