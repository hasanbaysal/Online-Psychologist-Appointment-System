namespace HB.OnlinePsikologMerkezi.Common.CustomResponse
{
    public class Response : IResponse
    {
        public string Message { get; set; } = "";
        public ResponseType ResponseType { get; set; }
        public Response(ResponseType responseType)
        {
            ResponseType = responseType;
        }
        public Response(ResponseType responseType, string message) : this(responseType)
        {
            Message = message;
        }

    }

}
