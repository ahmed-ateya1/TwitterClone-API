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
        public string Bio { get; set; }
        public string Gender { get; set; }
        public DateTime BirthDate { get; set; }
        public long TotalFollowing { get; set; } 
        public long TotalFollowers { get; set; } 
        public long TotalTweets { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime UpdatedAt { get; set; }
        public Guid UserID { get; set; }
        public string UserName { get; set; }
        public int Age { get; set; }
    }
}
