using HB.OnlinePsikologMerkezi.Common.CustomEnums;
using HB.OnlinePsikologMerkezi.Common.CustomResponse;
using HB.OnlinePsikologMerkezi.Dto.Dtos;

namespace HB.OnlinePsikologMerkezi.Business.Services
{
    public interface IAdminService
    {



        Task<Response<List<OrderListDto>>> GetAppointmentsByDate(OrderDate orderDate);
        Task<Response<PsychologistAddDto>> PsychologistRoleAddAsync(PsychologistAddDto dto);
        Task<Response<NoDataResponse>> PsychologistRoleRemoveAsync(string name);
        Task<Response<NoDataResponse>> UserBlockAsync(string name);
        Task<Response<NoDataResponse>> UserUnBlockAsync(string name);
        Task<Response<PsychologistUpdateDto>> PskUpdate(PsychologistUpdateDto dto);
        Task<Response<PsychologistUpdateDto>> GetPskForUpdate(string pskİd);
        /// <summary>
        /// burada dikkat etmen gereken şey psikolog tablosunda olmayan userları getirmek
        /// </summary>
        /// <param name="pagesize"></param>
        /// <param name="pagenumber"></param>
        /// <returns></returns>
        Task<List<AppUserListDto>> GetUserAsync(int pagesize, int pagenumber);
        Task<Response<AppUserListDto>> GetUserdetails(string userId);
        Task<Response<AppUserListDto>> GetUserByEmail(string email);



        //kullanıcıya mail at
        //blog ekle
        //blog güncelle
        //blog pasife al
        //kullanıcıya mesaj at

    }
}
