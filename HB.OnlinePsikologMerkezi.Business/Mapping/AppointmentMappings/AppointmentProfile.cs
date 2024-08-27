using AutoMapper;
using HB.OnlinePsikologMerkezi.Dto.Dtos;
using HB.OnlinePsikologMerkezi.Entities.Entities;

namespace HB.OnlinePsikologMerkezi.Business.Mapping;

public class AppointmentProfile : Profile
{
    public AppointmentProfile()
    {
        CreateMap<Appointment, AppointmentListDto>().ReverseMap();
    }
}
