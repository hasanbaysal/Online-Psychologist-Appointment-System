using AutoMapper;
using HB.OnlinePsikologMerkezi.Dto.Dtos;
using HB.OnlinePsikologMerkezi.Entities.Entities;

namespace HB.OnlinePsikologMerkezi.Business.Mapping.CategoryPskMappings
{
    public class CategoryPyschologistProfile : Profile
    {
        public CategoryPyschologistProfile()
        {
            CreateMap<CategoryPskListDto, PsychologistCategory>().ReverseMap();
            CreateMap<CategoryPskAddDto, PsychologistCategory>().ReverseMap();


        }
    }
}
