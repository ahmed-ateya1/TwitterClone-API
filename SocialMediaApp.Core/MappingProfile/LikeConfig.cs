using SocialMediaApp.Core.Domain.Entites;
using SocialMediaApp.Core.DTO.LikeDTO;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SocialMediaApp.Core.MappingProfile
{
    public class LikeConfig : AutoMapper.Profile
    {
        public LikeConfig()
        {
            CreateMap<Like , LikeResponse>()
                .ForMember(dest => dest.UserName, opt => opt.MapFrom(src => src.Profile.User.UserName))
                .ForMember(dest=>dest.ProfilePicture , opt=>opt.MapFrom(src => src.Profile.ProfileImgURL))
                .ForMember(dest=>dest.FullName, opt => opt.MapFrom(src => src.Profile.FullName))
                .ReverseMap();
        }
    }
}
