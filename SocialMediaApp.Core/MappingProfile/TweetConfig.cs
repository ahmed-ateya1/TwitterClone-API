using AutoMapper;
using SocialMediaApp.Core.Domain.Entites;
using SocialMediaApp.Core.DTO.TweetDTO;
using SocialMediaApp.Core.IUnitOfWorkConfig;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SocialMediaApp.Core.MappingProfile
{
    public class TweetConfig : AutoMapper.Profile
    {
        private readonly IUnitOfWork _unitOfWork;

        public TweetConfig(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }

        public TweetConfig()
        {

            CreateMap<TweetAddRequest, Tweet>()
                .ForMember(dest=>dest.CreatedAt, opt => opt.MapFrom(src => DateTime.UtcNow))
                .ForMember(dest=>dest.IsUpdated , opt => opt.MapFrom(src => false))
                .ForMember(dest => dest.TotalLikes, opt => opt.MapFrom(src => 0))
                .ForMember(dest => dest.TotalRetweets, opt => opt.MapFrom(src => 0))
                .ForMember(dest => dest.TotalComments, opt => opt.MapFrom(src => 0))
                .ForMember(dest => dest.UpdatedAt, opt => opt.MapFrom(src => DateTime.UtcNow))
                .ReverseMap();

            CreateMap<Tweet, TweetResponse>()
                .ForMember(dest => dest.UserName, opt => opt.MapFrom(src => src.Profile.User.UserName))
                .ForMember(dest => dest.FullName, opt => opt.MapFrom(src => src.Profile.FullName))
                .ForMember(dest => dest.ProfilePictureURL, opt => opt.MapFrom(src => src.Profile.ProfileImgURL))
                .ForMember(dest => dest.GenreName, opt => opt.MapFrom(src => src.Genre.GenreName))
                .ForMember(dest => dest.FilesURL, opt => opt.MapFrom(src => src.Files.Select(x => x.FileURL).ToList()))
                .ReverseMap();

            CreateMap<TweetUpdateRequest, Tweet>()
               .ForMember(dest => dest.CreatedAt, opt => opt.MapFrom(src => DateTime.UtcNow))
               .ForMember(dest => dest.IsUpdated, opt => opt.MapFrom(src => true))
               .ForMember(dest => dest.TotalLikes, opt => opt.MapFrom(src => 0))
               .ForMember(dest => dest.TotalRetweets, opt => opt.MapFrom(src => 0))
               .ForMember(dest => dest.TotalComments, opt => opt.MapFrom(src => 0))
               .ForMember(dest => dest.UpdatedAt, opt => opt.MapFrom(src => DateTime.UtcNow))
               .ReverseMap();
        }
    }
}
