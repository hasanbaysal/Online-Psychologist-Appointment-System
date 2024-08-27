namespace HB.OnlinePsikologMerkezi.Common.CustomResponse
{
    public interface IResponse
    {
        public string Message { get; set; }
        public ResponseType ResponseType { get; set; }

    }

    public enum ResponseType
    {
        Success = 1,
        Fail = 2,
        ValidationError = 3,
        NotFound = 4,
        IsnNotAllowed = 5,
        IsLockOut = 6,
        isBlock = 7,
        paymenterror=8
    }

}
