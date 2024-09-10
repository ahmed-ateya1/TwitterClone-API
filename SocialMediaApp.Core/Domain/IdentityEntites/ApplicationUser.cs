using Microsoft.AspNetCore.Identity;
using SocialMediaApp.Core.Domain.Entites;
using SocialMediaApp.Core.DTO.AuthenticationDTO;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SocialMediaApp.Core.Domain.IdentityEntites
{
    public class ApplicationUser : IdentityUser<Guid>
    {
        public Guid ProfileID { get; set; }
        public Profile Profile { get; set; }
        public ICollection<RefreshToken>? RefreshTokens { get; set; }
    }
}
