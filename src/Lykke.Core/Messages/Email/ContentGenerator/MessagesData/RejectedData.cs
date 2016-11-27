namespace Core.Messages.Email.ContentGenerator.MessagesData
{
    public class RejectedData : IEmailMessageData {
        public string MessageId()
        {
            return "RejectedEmail";
        }
    }
}
