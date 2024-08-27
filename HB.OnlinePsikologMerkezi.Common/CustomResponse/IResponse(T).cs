using HB.OnlinePsikologMerkezi.Common.CustomErros;

namespace HB.OnlinePsikologMerkezi.Common.CustomResponse
{
    public interface IResponse<T> : IResponse
    {
        public T? Data { get; set; }
        public List<CustomValidationError>? Errors { get; set; }
    }
}
