using HB.OnlinePsikologMerkezi.Common.CustomErros;

namespace HB.OnlinePsikologMerkezi.Common.CustomResponse
{
    public class Response<T> : Response, IResponse<T>
    {
        //sadece response type dönmek isteyebilirim
        public Response(ResponseType responseType) : base(responseType)
        {

        }
        //sadece reponse type ve message dönüyorum 
        public Response(ResponseType responseType, string message) : base(responseType, message)
        {

        }
        public Response(T data, List<CustomValidationError> errors) : this(ResponseType.ValidationError)
        {
            Errors = errors;
        }
        public Response(ResponseType type, T data) : this(type)
        {
            Data = data;
        }
        public T? Data { get; set; }
        public List<CustomValidationError>? Errors { get; set; }

    }
}
