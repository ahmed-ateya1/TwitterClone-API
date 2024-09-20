using SocialMediaApp.Core.Domain.Entites;
using SocialMediaApp.Core.DTO.NotificationDTO;

namespace SocialMediaApp.Core.MappingProfile
{
    public class NotificationConfig : AutoMapper.Profile
    {
        public NotificationConfig()
        {
            CreateMap<NotificationAddRequest, Notification>()
                .ForMember(dest => dest.CreatedAt, opt => opt.MapFrom(src => DateTime.UtcNow))
                .ForMember(dest => dest.IsRead, opt => opt.MapFrom(src => false))
                .ForMember(dest=>dest.NotificationType, opt => opt.MapFrom(src => src.NotificationType.ToString()))
                .ReverseMap();

            CreateMap<Notification, NotificationResponse>()
                .ForMember(dest => dest.ProfilePicture, opt => opt.MapFrom(src => src.Profile.ProfileImgURL))
                .ForMember(dest => dest.UserName, opt => opt.MapFrom(src => src.Profile.User.UserName))
                .ReverseMap();
        }
    }
}
