namespace Core.Messages.Email.ContentGenerator.MessagesData
{
    public class BankCashInData : IEmailMessageData
    {
        public string AssetId { get; set; }
        public double Amount { get; set; }
        public string ClientId { get; set; }

        public string MessageId()
        {
            return "BankCashInEmail";
        }
    }
}
