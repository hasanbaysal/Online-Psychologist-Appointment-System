using HB.OnlinePsikologMerkezi.Common.Options;
using Microsoft.Extensions.Options;
using System.Net;
using System.Net.Mail;

namespace HB.OnlinePsikologMerkezi.Common.Mail
{
    public class MailManager : IMailService
    {
        private readonly EmailOptions emailOptions;

        public MailManager(IOptions<EmailOptions> emailOptions)
        {
            this.emailOptions = emailOptions.Value;
        }

        //optimize edilebilir kodsal design açıdan ==> daha sonra bak open-close prensibi X
        public async Task SendMail(MailType type, string Reciver, string MessageBody, string Subject)
        {
            var smtpClient = new SmtpClient();
            smtpClient.EnableSsl = true;
            smtpClient.DeliveryMethod = SmtpDeliveryMethod.Network;
            smtpClient.UseDefaultCredentials = false;
            var mailMessage = new MailMessage();

            if (type == MailType.Service)
            {
                smtpClient.Host = emailOptions.ServiceEmailSettings.Host;
                smtpClient.Port = emailOptions.ServiceEmailSettings.Port;

                smtpClient.Credentials = new NetworkCredential(emailOptions.ServiceEmailSettings.Email, emailOptions.ServiceEmailSettings.Password);

                mailMessage.From = new MailAddress(emailOptions.ServiceEmailSettings.Email);
            }
            if (type == MailType.Notification)
            {
                smtpClient.Host = emailOptions.NotificationEmailSettings.Host;
                smtpClient.Port = emailOptions.NotificationEmailSettings.Port;

                smtpClient.Credentials = new NetworkCredential(emailOptions.NotificationEmailSettings.Email, emailOptions.NotificationEmailSettings.Password);

                mailMessage.From = new MailAddress(emailOptions.NotificationEmailSettings.Email);
            }
            if (type == MailType.Support)
            {
                smtpClient.Host = emailOptions.SupportEmailSettings.Host;
                smtpClient.Port = emailOptions.SupportEmailSettings.Port;

                smtpClient.Credentials = new NetworkCredential(emailOptions.SupportEmailSettings.Email,
                    emailOptions.SupportEmailSettings.Password);

                mailMessage.From = new MailAddress(emailOptions.SupportEmailSettings.Email);
            }

            mailMessage.To.Add(Reciver);
            mailMessage.Subject = Subject;
            mailMessage.Body = MessageBody;
            mailMessage.IsBodyHtml = true;

            await smtpClient.SendMailAsync(mailMessage);

        }
    }
    public enum MailType
    {
        Service,
        Notification,
        Support
    }
}
