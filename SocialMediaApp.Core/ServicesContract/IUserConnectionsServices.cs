using SocialMediaApp.Core.DTO.UserConnectionsDTO;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SocialMediaApp.Core.ServicesContract
{
    public interface IUserConnectionsServices 
    {
        Task<UserConnectionsResponse> FollowAsync(Guid followedId);
        Task UnfollowAsync(Guid UnfollowedId);
    }
}
