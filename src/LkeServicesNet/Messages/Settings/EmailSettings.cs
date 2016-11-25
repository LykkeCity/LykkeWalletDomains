namespace LkeServicesNet.Messages.Settings
{
    public class EmailSettings
    {
        public string SmtpHost { get; set; }
        public int SmtpPort { get; set; }
        public string SmtpLogin { get; set; }
        public string SmtpPwd { get; set; }
        public string EmailFrom { get; set; }
        public string EmailFromDisplayName { get; set; }
    }
}
