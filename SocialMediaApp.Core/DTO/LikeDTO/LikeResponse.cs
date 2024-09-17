using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SocialMediaApp.Core.DTO.LikeDTO
{
    public class LikeResponse
    {
        public Guid LikeID { get; set; }
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public string UserName { get; set; }
        public string FullName { get; set; }
        public string ProfilePicture { get; set; }
    }
}
