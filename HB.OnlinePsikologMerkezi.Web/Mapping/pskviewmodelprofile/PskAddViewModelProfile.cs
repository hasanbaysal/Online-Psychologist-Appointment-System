using AutoMapper;
using HB.OnlinePsikologMerkezi.Dto.Dtos;
using HB.OnlinePsikologMerkezi.Web.Areas.Admin.Models;

namespace HB.OnlinePsikologMerkezi.Web.Mapping.pskviewmodelprofile
{
    public class PskAddViewModelProfile : Profile
    {
        public PskAddViewModelProfile()
        {
            CreateMap<PskAddViewModel, PsychologistAddDto>().ReverseMap();
            CreateMap<PsychologistUpdateDto, PskUpdateViewModel>().ReverseMap();
        }
    }
}
