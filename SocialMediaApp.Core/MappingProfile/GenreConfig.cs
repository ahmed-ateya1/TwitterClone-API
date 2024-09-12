using AutoMapper;
using SocialMediaApp.Core.Domain.Entites;
using SocialMediaApp.Core.DTO.GenreDTO;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SocialMediaApp.Core.MappingProfile
{
    public class GenreConfig : AutoMapper.Profile
    {
        public GenreConfig()
        {
            CreateMap<GenreAddRequest , Genre>().ReverseMap();
            CreateMap<GenreUpdateRequest, Genre>().ReverseMap();
            CreateMap<Genre, GenreResponse>().ReverseMap();
            CreateMap<Genre , Genre>().ReverseMap();
        }
    }
}
