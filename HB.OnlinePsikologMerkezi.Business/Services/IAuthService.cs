using HB.OnlinePsikologMerkezi.Common.CustomResponse;
using HB.OnlinePsikologMerkezi.Dto.Dtos;
using Microsoft.AspNetCore.Mvc;

namespace HB.OnlinePsikologMerkezi.Business.Services
{
    public interface IAuthService
    {
        Task<Response<UserSignUpDto>> SingUpAsync(UserSignUpDto upDto);
        Task<Response> AccountActivationAsync(string userid, string token);
        Task<ChallengeResult> LoginWithGoole(string redirectUrl);
        Task<Response<NoDataResponse>> ExternalLogin(string redirectUrl = "/");
        Task<Response<UserSigInDto>> SingInWithPassword(UserSigInDto dto);
        Task<Response<ForgetPasswordDto>> ForgetPasswordSendMail(ForgetPasswordDto dto);
        Task<bool> ConfrimUserIdAndResetToken(string userid, string token);
        Task<Response<ResetPasswordDto>> ResetPassord(ResetPasswordDto dto, string userId, string token);

    }
}
