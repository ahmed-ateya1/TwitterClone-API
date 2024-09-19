using SocialMediaApp.Core.Domain.Entites;
using SocialMediaApp.Core.DTO.CommentDTO;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SocialMediaApp.Core.MappingProfile
{
    public class CommentConfig : AutoMapper.Profile
    {
        public CommentConfig()
        {
            CreateMap<CommentAddRequest, Comment>()
                .ReverseMap();
            CreateMap<CommentUpdateRequest, Comment>()
                .ForMember(x=>x.IsUpdated, opt => opt.MapFrom(src => true))
                .ReverseMap();
            CreateMap<Comment, CommentResponse>()
                .ForMember(dest => dest.FilesUrl, opt => opt.MapFrom(src => src.Files.Select(x => x.FileUrl).ToList()))
                .ForMember(dest => dest.ProfileImageUrl, opt => opt.MapFrom(dest => dest.Profile.ProfileBackgroundURL))
                .ForMember(dest => dest.FullName, opt => opt.MapFrom(dest => dest.Profile.FullName))
                .ForMember(dest => dest.UserName, opt => opt.MapFrom(src => src.Profile.User.UserName));
        }
    }
}
