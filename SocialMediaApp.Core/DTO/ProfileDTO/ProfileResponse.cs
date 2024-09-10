using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SocialMediaApp.Core.DTO.ProfileDTO
{
    public class ProfileResponse
    {
        public Guid ProfileID { get; set; }
        public string ProfileImgURL { get; set; }
        public string ProfileBackgroundURL { get; set; }
        public string Gender { get; set; }
        public DateTime BirthDate { get; set; }
        public long TotalFollowing { get; set; } = 0;
        public long TotalFollowers { get; set; } = 0;
        public long TotalTweets { get; set; } = 0;
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
        public Guid UserID { get; set; }
        public string UserName { get; set; }
        public int Age { get; set; }
    }
}
