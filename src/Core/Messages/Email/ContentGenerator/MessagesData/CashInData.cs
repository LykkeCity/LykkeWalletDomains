namespace Core.Messages.Email.ContentGenerator.MessagesData
{
    public class CashInData : IEmailMessageData
    {
        public string Multisig { get; set; }
        public string AssetId { get; set; }

        public string MessageId()
        {
            return "CashInEmail";
        }
    }
}
