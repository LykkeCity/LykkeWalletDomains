namespace Core.Messages.Email.ContentGenerator.MessagesData
{
    public class UserRegisteredData : IEmailMessageData
    {
        public string ClientId { get; set; }
        public string MessageId()
        {
            return "UserRegisteredBroadcast";
        }
    }
}
