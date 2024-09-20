using AutoMapper;
using SocialMediaApp.Core.Domain.Entites;
using SocialMediaApp.Core.DTO.UserConnectionsDTO;

namespace SocialMediaApp.Core.MappingProfile
{
    public class UserConnectionsConfig : AutoMapper.Profile
    {
        public UserConnectionsConfig() {

            CreateMap<UserConnections, FollowResponse>()
                .ForMember(dest => dest.UserConnectionId, opt => opt.MapFrom(prop => prop.UserConnectionID))
                .ForMember(dest => dest.CreatedAt, opt => opt.MapFrom(prop => prop.CreatedAt))
                .ForMember(dest => dest.FollowerID, opt => opt.MapFrom(prop => prop.FollowerID))
                .ForMember(dest => dest.FollowedID, opt => opt.MapFrom(prop => prop.FollowedID))
                .ReverseMap();

            CreateMap<Domain.Entites.Profile, UserConnectionsResponse>()
                .ForMember(dest => dest.Id, opt=> opt.MapFrom(prop => prop.ProfileID))
                .ForMember(dest => dest.FullName, opt => opt.MapFrom(prop => prop.FullName))
                .ForMember(dest => dest.UserName, opt => opt.MapFrom(prop=> prop.User.UserName))
                .ForMember(dest => dest.Bio, opt => opt.MapFrom(prop => prop.Bio))
                .ForMember(dest => dest.ProfileImg, opt => opt.MapFrom(prop => prop.ProfileImgURL))
                .ReverseMap();
        }
        
    }
}
