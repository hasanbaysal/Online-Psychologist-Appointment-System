using AutoMapper;
using HB.OnlinePsikologMerkezi.Dto.Dtos;
using HB.OnlinePsikologMerkezi.Entities.Entities;

namespace HB.OnlinePsikologMerkezi.Business.Mapping
{
    public class PsychologistProfile : Profile
    {
        public PsychologistProfile()
        {
            CreateMap<Psychologist, PsychologistListDto>().ReverseMap();
            CreateMap<Psychologist, PsychologistAddDto>().ReverseMap();
            CreateMap<Psychologist, PsychologistUpdateDto>().ReverseMap();

        }
    }
}
