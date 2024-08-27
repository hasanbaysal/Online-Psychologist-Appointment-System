using AutoMapper;
using HB.OnlinePsikologMerkezi.Dto.Dtos;
using HB.OnlinePsikologMerkezi.Entities.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HB.OnlinePsikologMerkezi.Business.Mapping.BlogMappings
{
    public class BlogProfile:Profile
    {
        public BlogProfile()
        {

            CreateMap<Blog, BlogAddDto>().ReverseMap();
            CreateMap<Blog, BlogUpdateDto>().ReverseMap();
        }
    }
}
