namespace Core.Messages.Email.ContentGenerator.MessagesData
{
    public class PrivateWalletBackupData : IEmailMessageData
    {
        public string WalletName { get; set; }
        public string WalletAddress { get; set; }
        public string SecurityQuestion { get; set; }
        public string EncodedKey { get; set; }
        public string MessageId()
        {
            return "PrivateWalletBackupEmail";
        }
    }
}
