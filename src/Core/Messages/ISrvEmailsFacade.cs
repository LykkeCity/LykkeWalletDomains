using System.Threading.Tasks;
using Core.Broadcast;

namespace Core.Messages
{
    public interface ISrvEmailsFacade
    {
        Task SendWelcomeEmail(string email, string clientId);

        Task SendWelcomeFxEmail(string email, string clientId);

        Task SendConfirmEmail(string email, bool generateRealCode);

        Task SendUserRegisteredKycBroadcast(string clientId);

        Task SendRejectedEmail(string email);

        Task SendNoRefundDepositDoneMail(string email, double amount, string assetBcnId);

        Task SendNoRefundOCashOutMail(string email, double amount, string assetId, string bcnHash);

        Task SendSwiftEmail(string email, string clientId, double balanceChange, string assetId);

        #region Refund messages

        Task SendCashInRefundMail(string email, double amount, string srcBlockchainHash,
            string refundTransaction, int validDays);

        Task SendSwapRefundMail(string email, double amount, string srcBlockchainHash,
            string refundTransaction, int validDays);

        Task SendOCashOutRefundMail(string email, string assetId, double amount, string srcBlockchainHash,
            string refundTransaction, int validDays);

        #endregion

        Task SendFailedTransactionBroadcast(string transactionId, string[] affectedClientIds);

        Task SendTransferCompletedEmail(string email, string clientName, string assetId, double amountFiat,
            double amountLkk, double price, string srcHash);

        Task SendDirectTransferCompletedEmail(string email, string clientName, string assetId, double amount, string srcHash);

        Task SendBackupPrivateWalletEmail(string email, string walletName, string walletAddress,
            string question, string encodedKey);

        Task SendRemindPasswordEmail(string email, string hint);

        Task SendPlainTextBroadcast(BroadcastGroup @group, string subject, string text);
    }
}
