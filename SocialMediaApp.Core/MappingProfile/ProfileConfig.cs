using AutoMapper;
using SocialMediaApp.Core.DTO.ProfileDTO;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SocialMediaApp.Core.MappingProfile
{
    public class ProfileConfig : Profile
    {
        public ProfileConfig()
        {
            CreateMap<ProfileAddRequest, Domain.Entites.Profile>()
                .ForMember(dest => dest.FullName, opt => opt.MapFrom(src => src.FirstName + " " + src.LastName))
                .ForMember(dest => dest.CreatedAt, opt => opt.MapFrom(src => DateTime.UtcNow))
                .ForMember(dest => dest.TotalFollowers, opt => opt.MapFrom(src => 0))
                .ForMember(dest => dest.TotalFollowing, opt => opt.MapFrom(src => 0))
                .ForMember(dest => dest.TotalTweets, opt => opt.MapFrom(src => 0));


            CreateMap<Domain.Entites.Profile, ProfileResponse>()
                .ForMember(dest => dest.UserName, opt => opt.MapFrom(src => src.User.UserName))
                .ForMember(dest => dest.Age, opt => opt.MapFrom(src => DateTime.UtcNow.Year - src.BirthDate.Year))
                .ReverseMap();

            CreateMap<ProfileUpdateRequest, Domain.Entites.Profile>()
                .ForMember(dest => dest.FullName, opt => opt.MapFrom(src => src.FirstName + " " + src.LastName))
                .ForMember(dest => dest.UpdatedAt, opt => opt.MapFrom(src => DateTime.UtcNow));
        }
    }
}
