using System.Threading.Tasks;
using Core.Clients;
using Core.Messages.Email.ContentGenerator.MessagesData;
using Core.Messages.Email.Sender;

namespace Core.Messages.Email.ContentGenerator
{
    public interface IEmailGenerator
    {
        Task<EmailMessage> GenerateWelcomeMsg(RegistrationData kycOkData);
        Task<EmailMessage> GenerateWelcomeFxMsg(KycOkData kycOkData);
        Task<EmailMessage> GenerateConfirmEmailMsg(EmailComfirmationData registrationData);
        Task<EmailMessage> GenerateCashInMsg(CashInData messageData);
        Task<EmailMessage> GenerateNoRefundDepositDoneMsg(NoRefundDepositDoneData messageData);
        Task<EmailMessage> GenerateNoRefundOCashOutMsg(NoRefundOCashOutData messageData);
        Task<EmailMessage> GenerateBankCashInMsg(BankCashInData messageData);
        Task<EmailMessage> GenerateCashInRefundMsg(CashInRefundData messageData);
        Task<EmailMessage> GenerateUserRegisteredMsg(IPersonalData messageData);
        Task<EmailMessage> GenerateRejectedEmailMsg();
        EmailMessage GenerateFailedTransactionMsg(string transactionId, string[] clientIds);
        Task<EmailMessage> GenerateSwapRefundMsg(SwapRefundData messageData);
        Task<EmailMessage> GenerateOrdinaryCashOutRefundMsg(OrdinaryCashOutRefundData messageData);
        Task<EmailMessage> GenerateTransferCompletedMsg(TransferCompletedData transferCompletedData);
        Task<EmailMessage> GenerateDirectTransferCompletedMsg(DirectTransferCompletedData transferCompletedData);
        Task<EmailMessage> GeneratePrivateWalletBackupMsg(PrivateWalletBackupData privateWalletBackupData);
        Task<EmailMessage> GenerateMyLykkeCashInMsg(MyLykkeCashInData messageData);
        Task<EmailMessage> GenerateRemindPasswordMsg(RemindPasswordData messageData);
        Task<EmailMessage> GeneratPrivateWalletAddressMsg(PrivateWalletAddressData messageData);
    }
}
