namespace HB.OnlinePsikologMerkezi.Common.Mail
{
    public interface IMailService
    {
        Task SendMail(MailType type, string Reciver, string MessageBody, string Subject);
    }
}
