namespace Core.Messages.Email.ContentGenerator.MessagesData
{
    public class KycOkData : IEmailMessageData
    {
        public string ClientId { get; set; }
        public string Year { get; set; }
        public string MessageId()
        {
            return "WelcomeFxEmail";
        }
    }
}
