using HB.OnlinePsikologMerkezi.Common.CustomResponse;
using HB.OnlinePsikologMerkezi.Dto.Dtos;

namespace HB.OnlinePsikologMerkezi.Business.Services
{
    public interface IPaymentService
    {
        Task<Response<NoDataResponse>> BeginPayment(PaymentInformationDto paymentDto, string userName, int appointmentId);
        Task<bool> CheckAppointmentStatus(int id);

        Task<Response<NoDataResponse>> BeginPayment3D(PaymentInformationDto paymentDto, string userName, int appointmentId);

        Task<bool> Complate3dSecureOperation(string conversationId, string paymentid);
        bool Control3dSecureResult(string paymentid);


    }
}
