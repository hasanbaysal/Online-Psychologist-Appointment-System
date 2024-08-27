using HB.OnlinePsikologMerkezi.Common.CustomEnums;
using HB.OnlinePsikologMerkezi.Common.CustomResponse;
using HB.OnlinePsikologMerkezi.Dto.Dtos;

namespace HB.OnlinePsikologMerkezi.Business.Services
{
    public interface IPsychologistService
    {
        Task<Response<NoDataResponse>> AddAppointment(AppointmentAddDto dto);
        Task<Response<NoDataResponse>> RemoveAppointment(string pskid, int id);
        Task<Response<List<AppointmentListDto>>> GetAppointmensASync(AppointmentEnum type, int secondkey);
        Task<Response<List<PsychologistListDto>>> GetPsycholistsWithCategort();



    }
}
