using AutoMapper;
using HB.OnlinePsikologMerkezi.Dto.Dtos;
using HB.OnlinePsikologMerkezi.Entities.Entities;

namespace HB.OnlinePsikologMerkezi.Business.Mapping.CategoryMappings
{
    public class CategoryProfile : Profile
    {
        public CategoryProfile()
        {
            CreateMap<Category, CategoryListDto>().ReverseMap();
            CreateMap<Category, CategoryAddDto>().ReverseMap();
        }
    }
}
