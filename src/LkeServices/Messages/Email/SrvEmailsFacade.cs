using System;
using System.Threading.Tasks;
using Core.Broadcast;
using Core.Messages;
using Core.Messages.Email.ContentGenerator.MessagesData;
using Core.Messages.Email.Sender;
using Core.VerificationCode;

namespace LkeServices.Messages.Email
{
    public class SrvEmailsFacade : ISrvEmailsFacade
    {
        private readonly IEmailVerificationCodeRepository _emailVerificationCodeRepository;
        private readonly IEmailSender _emailSender;

        public SrvEmailsFacade(IEmailVerificationCodeRepository emailVerificationCodeRepository,
            IEmailSender emailSender)
        {
            _emailVerificationCodeRepository = emailVerificationCodeRepository;
            _emailSender = emailSender;
        }

        public async Task SendWelcomeEmail(string email, string clientId)
        {
            var msgData = new RegistrationData
            {
                ClientId = clientId,
                Year = DateTime.UtcNow.Year.ToString()
            };
            await _emailSender.SendEmailAsync(email, msgData);
        }

        public async Task SendWelcomeFxEmail(string email, string clientId)
        {
            var msgData = new KycOkData
            {
                ClientId = clientId,
                Year = DateTime.UtcNow.Year.ToString()
            };
            await _emailSender.SendEmailAsync(email, msgData);
        }

        public async Task SendConfirmEmail(string email, bool generateRealCode)
        {
            var insertedCode = await _emailVerificationCodeRepository.CreateAsync(email, generateRealCode);

            var msgData = new EmailComfirmationData()
            {
                ConfirmationCode = insertedCode.Code,
                Year = DateTime.UtcNow.Year.ToString()
            };

            await _emailSender.SendEmailAsync(email, msgData);
        }

        public async Task SendUserRegisteredKycBroadcast(string clientId)
        {
            var broadcastMsg = new UserRegisteredData() { ClientId = clientId };
            await _emailSender.SendEmailBroadcastAsync(BroadcastGroup.Kyc, broadcastMsg);
        }

        public async Task SendRejectedEmail(string email)
        {
            var msgData = new RejectedData();
            await _emailSender.SendEmailAsync(email, msgData);
        }

        public async Task SendNoRefundDepositDoneMail(string email, double amount, string assetBcnId)
        {
            var msgData = new NoRefundDepositDoneData
            {
                Amount = amount,
                AssetBcnId = assetBcnId
            };
            await _emailSender.SendEmailAsync(email, msgData);
        }

        public async Task SendNoRefundOCashOutMail(string email, double amount, string assetId, string bcnHash)
        {
            var msgData = new NoRefundOCashOutData
            {
                Amount = amount,
                AssetId = assetId,
                SrcBlockchainHash = bcnHash
            };

            await _emailSender.SendEmailAsync(email, msgData);
        }

        public async Task SendSwiftEmail(string email, string clientId, double balanceChange, string assetId)
        {
            var msgData = new BankCashInData
            {
                ClientId = clientId,
                Amount = balanceChange,
                AssetId = assetId
            };

            await _emailSender.SendEmailAsync(email, msgData);
        }

        #region Refund messages

        public async Task SendCashInRefundMail(string email, double amount, string srcBlockchainHash, string refundTransaction, int validDays)
        {
            var msgData = new CashInRefundData
            {
                Amount = amount,
                SrcBlockchainHash = srcBlockchainHash,
                RefundTransaction = refundTransaction,
                ValidDays = validDays
            };
            await _emailSender.SendEmailAsync(email, msgData);
        }

        public async Task SendSwapRefundMail(string email, double amount, string srcBlockchainHash, string refundTransaction, int validDays)
        {
            var msgData = new SwapRefundData
            {
                Amount = amount,
                SrcBlockchainHash = srcBlockchainHash,
                RefundTransaction = refundTransaction,
                ValidDays = validDays
            };
            await _emailSender.SendEmailAsync(email, msgData);
        }

        public async Task SendOCashOutRefundMail(string email, string assetId, double amount, string srcBlockchainHash, string refundTransaction, int validDays)
        {
            var msgData = new OrdinaryCashOutRefundData
            {
                AssetId = assetId,
                Amount = amount,
                SrcBlockchainHash = srcBlockchainHash,
                RefundTransaction = refundTransaction,
                ValidDays = validDays
            };
            await _emailSender.SendEmailAsync(email, msgData);
        }

        #endregion

        public async Task SendFailedTransactionBroadcast(string transactionId, string[] affectedClientIds)
        {
            var msgData = new FailedTransactionData
            {
                TransactionId = transactionId,
                AffectedClientIds = affectedClientIds
            };
            await _emailSender.SendEmailBroadcastAsync(BroadcastGroup.Errors, msgData);
        }

        public async Task SendTransferCompletedEmail(string email, string clientName, string assetId, double amountFiat, double amountLkk,
            double price, string srcHash)
        {
            var msgData = new TransferCompletedData
            {
                AssetId = assetId,
                AmountFiat = amountFiat,
                AmountLkk = amountLkk,
                Price = price,
                ClientName = clientName,
                SrcBlockchainHash = srcHash
            };
            await _emailSender.SendEmailAsync(email, msgData);
        }

        public async Task SendDirectTransferCompletedEmail(string email, string clientName, string assetId, double amount, string srcHash)
        {
            var msgData = new DirectTransferCompletedData
            {
                AssetId = assetId,
                Amount = amount,
                ClientName = clientName,
                SrcBlockchainHash = srcHash
            };

            await _emailSender.SendEmailAsync(email, msgData);
        }

        public async Task SendBackupPrivateWalletEmail(string email, string walletName, string walletAddress,
            string question, string encodedKey)
        {
            var msgData = new PrivateWalletBackupData
            {
                WalletName = walletName,
                WalletAddress = walletAddress,
                EncodedKey = encodedKey,
                SecurityQuestion = question
            };
            await _emailSender.SendEmailAsync(email, msgData);
        }

        public async Task SendRemindPasswordEmail(string email, string hint)
        {
            var msgData = RemindPasswordData.Create(hint);

            await _emailSender.SendEmailAsync(email, msgData);
        }

        public async Task SendPlainTextBroadcast(BroadcastGroup @group, string subject, string text)
        {
            await
                _emailSender.SendEmailBroadcastAsync(@group,
                    new PlainTextBroadCastData { Subject = subject, Text = text});
        }
    }
}
