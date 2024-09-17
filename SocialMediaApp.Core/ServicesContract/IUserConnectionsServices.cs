using SocialMediaApp.Core.Domain.Entites;
using SocialMediaApp.Core.DTO.UserConnectionsDTO;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;

namespace SocialMediaApp.Core.ServicesContract
{
    public interface IUserConnectionsServices 
    {
        Task<FollowResponse> FollowAsync(Guid followedId);
        Task UnfollowAsync(Guid UnfollowedId);
        Task<List<UserConnectionsResponse>> GetUserFollowingAsync(Guid profileId , int? pageIndex , int? pageSize);
        Task<List<UserConnectionsResponse>> GetUserFollowersAsync(Guid profileId , int? pageIndex , int? pageSize);

    }
}
