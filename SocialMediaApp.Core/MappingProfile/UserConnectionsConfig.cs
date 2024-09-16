using AutoMapper;
using SocialMediaApp.Core.Domain.Entites;
using SocialMediaApp.Core.DTO.UserConnectionsDTO;

namespace SocialMediaApp.Core.MappingProfile
{
    public class UserConnectionsConfig : AutoMapper.Profile
    {
        public UserConnectionsConfig() {

            CreateMap<UserConnections, UserConnectionsResponse>()
                .ForMember(dest => dest.UserConnectionId, opt => opt.MapFrom(prop => prop.UserConnectionID))
                .ForMember(dest => dest.CreatedAt, opt => opt.MapFrom(prop => prop.CreatedAt))
                .ForMember(dest => dest.FollowerID, opt => opt.MapFrom(prop => prop.FollowerID))
                .ForMember(dest => dest.FollowedID, opt => opt.MapFrom(prop => prop.FollowedID))
                .ReverseMap();
        }
    }
}
