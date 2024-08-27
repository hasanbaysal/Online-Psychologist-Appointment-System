using HB.OnlinePsikologMerkezi.Common.CustomResponse;
using HB.OnlinePsikologMerkezi.Dto.Dtos;

namespace HB.OnlinePsikologMerkezi.Business.Services
{
    public interface IUserService
    {

        //update password
        //send report message

        Task<Response<NoDataResponse>> UserUpdate(UserUpdateDto dto);
        Task<Response<List<OrderListDto>>> GetUserOrders(string userid);


    }
}
