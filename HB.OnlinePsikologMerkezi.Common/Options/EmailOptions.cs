namespace HB.OnlinePsikologMerkezi.Common.Options
{
    public class EmailOptions
    {

        public EmailSettings NotificationEmailSettings { get; set; } = null!;
        public EmailSettings ServiceEmailSettings { get; set; } = null!;
        public EmailSettings SupportEmailSettings { get; set; } = null!;


        public class EmailSettings
        {

            public string Host { get; set; } = null!;
            public string Email { get; set; } = null!;
            public string Password { get; set; } = null!;
            public int Port { get; set; }
        }
    }
}
