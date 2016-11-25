using System;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Common;
using Core;
using Core.Accounts.PrivateWallets;
using Core.Assets;
using Core.Clients;
using Core.Messages;
using Core.Messages.Email.ContentGenerator;
using Core.Messages.Email.ContentGenerator.MessagesData;
using Core.Messages.Email.Sender;
using Core.QrCode;
using Core.Settings;
using LkeServices.Messages.EmailTemplates.ViewModels;
using LkeServices.Messages.Resources;
using LkeServices.Messages.Settings;
using LkeServices.Pdf;

namespace LkeServices.Messages
{
    public class EmailGenerator : IEmailGenerator
    {
        private readonly SrvPdfGenerator _srvPdfGenerator;
        private readonly IAssetsRepository _assetsRepository;
        private readonly IPersonalDataRepository _personalDataRepository;
        private readonly ITemplateGenerator _templateGenerator;
        private readonly EmailGeneratorSettings _emailGeneratorSettings;
        private readonly IQrCodeGenerator _qrCodeGenerator;
        private readonly IBackupQrRepository _backupQrRepository;
        private readonly TemplateGenerator _localTemplateGenerator;
        private readonly BaseSettings _settings;

        public EmailGenerator(SrvPdfGenerator srvPdfGenerator, IAssetsRepository assetsRepository,
            IPersonalDataRepository personalDataRepository, TemplateGenerator localTemplateGenerator,
            BaseSettings settings, ITemplateGenerator templateGenerator, EmailGeneratorSettings emailGeneratorSettings,
            IQrCodeGenerator qrCodeGenerator, IBackupQrRepository backupQrRepository)
        {
            _srvPdfGenerator = srvPdfGenerator;
            _assetsRepository = assetsRepository;
            _personalDataRepository = personalDataRepository;
            _settings = settings;
            _templateGenerator = templateGenerator;
            _emailGeneratorSettings = emailGeneratorSettings;
            _qrCodeGenerator = qrCodeGenerator;
            _backupQrRepository = backupQrRepository;
            _localTemplateGenerator = localTemplateGenerator;
        }

        public async Task<EmailMessage> GenerateWelcomeMsg(RegistrationData kycOkData)
        {
            var templateVm = new BaseTemplate
            {
                Year = kycOkData.Year
            };

            return new EmailMessage
            {
                Body = await _templateGenerator.GenerateAsync("WelcomeTemplate", templateVm, TemplateType.Email),
                Subject = EmailResources.Welcome_Subject,
                IsHtml = true
            };
        }

        public async Task<EmailMessage> GenerateWelcomeFxMsg(KycOkData kycOkData)
        {
            var templateVm = new BaseTemplate
            {
                Year = kycOkData.Year
            };

            return new EmailMessage
            {
                Body = await _templateGenerator.GenerateAsync("WelcomeFxTemplate", templateVm, TemplateType.Email),
                Subject = EmailResources.WelcomeFx_Subject,
                IsHtml = true
            };
        }

        public async Task<EmailMessage> GenerateConfirmEmailMsg(EmailComfirmationData registrationData)
        {
            var templateVm = new EmailVerificationTemplate()
            {
                ConfirmationCode = registrationData.ConfirmationCode,
                Year = registrationData.Year
            };

            return new EmailMessage()
            {
                Body = await _templateGenerator.GenerateAsync("EmailConfirmation", templateVm, TemplateType.Email),
                Subject = EmailResources.EmailConfirmation_Subject,
                IsHtml = true
            };
        }

        public async Task<EmailMessage> GenerateCashInMsg(CashInData cashInData)
        {
            if (cashInData.AssetId == null)
                throw new ArgumentNullException("AssetId");

            var asset = await _assetsRepository.GetAssetAsync(cashInData.AssetId);
            var templateVm = new CashInTemplate()
            {
                Multisig = cashInData.Multisig,
                Year = DateTime.UtcNow.Year.ToString(),
                AssetName = asset.Id == LykkeConstants.LykkeAssetId ? EmailResources.LykkeCoins_name : asset.Name
            };

            return new EmailMessage()
            {
                Body = await _templateGenerator.GenerateAsync("CashInTemplate", templateVm, TemplateType.Email),
                Subject = string.Format(EmailResources.CashIn_Subject, templateVm.AssetName),
                IsHtml = true
            };
        }

        public async Task<EmailMessage> GenerateNoRefundDepositDoneMsg(NoRefundDepositDoneData messageData)
        {
            var asset = messageData.AssetBcnId != null
                ? await _assetsRepository.FindAssetByBlockchainAssetIdAsync(messageData.AssetBcnId)
                : await _assetsRepository.GetAssetAsync(LykkeConstants.BitcoinAssetId);

            var templateVm = new BtcDepositDoneTempate
            {
                AssetName = asset.Id == LykkeConstants.LykkeAssetId ? EmailResources.LykkeCoins_name : asset.Name,
                Amount = messageData.Amount,
                Year = DateTime.UtcNow.Year
            };

            var emailMessage = new EmailMessage
            {
                Body = await _templateGenerator.GenerateAsync("NoRefundDepositDoneTemplate", templateVm, TemplateType.Email),
                Subject = string.Format(EmailResources.Deposit_no_refund_done_subject, templateVm.AssetName),
                IsHtml = true
            };

            return emailMessage;
        }

        public async Task<EmailMessage> GenerateNoRefundOCashOutMsg(NoRefundOCashOutData messageData)
        {
            var templateVm = new NoRefundCashOutTemplate
            {
                AssetId = messageData.AssetId,
                Amount = messageData.Amount,
                ExplorerUrl = string.Format(_settings.BlockChainExplorerUrl, messageData.SrcBlockchainHash),
                Year = DateTime.UtcNow.Year
            };

            var emailMessage = new EmailMessage
            {
                Body = await _templateGenerator.GenerateAsync("NoRefundOCashOutTemplate", templateVm, TemplateType.Email),
                Subject = EmailResources.Cash_out_no_refund_subject,
                IsHtml = true
            };

            return emailMessage;
        }

        public async Task<EmailMessage> GenerateBankCashInMsg(BankCashInData messageData)
        {
            var personalData = await _personalDataRepository.GetAsync(messageData.ClientId);
            var asset = await _assetsRepository.GetAssetAsync(messageData.AssetId);

            var templateVm = new BankCashInTemplate
            {
                AssetId = messageData.AssetId,
                AssetSymbol = asset.Symbol,
                ClientName = personalData.FullName,
                Amount = messageData.Amount,
                Year = DateTime.UtcNow.Year.ToString(),
                AccountName = _settings.SwiftCredentials.AccountName,
                AccountNumber = _settings.SwiftCredentials.GetAccountNumber(messageData.AssetId),
                Bic = _settings.SwiftCredentials.BIC,
                PurposeOfPayment = string.Format(_settings.SwiftCredentials.PurposeOfPayment, messageData.AssetId, personalData.Email.Replace("@", ".")),
                BankAddress = _settings.SwiftCredentials.BankAddress,
                CompanyAddress = _settings.SwiftCredentials.CompanyAddress
            };

            var msg = new EmailMessage()
            {
                Body = await _templateGenerator.GenerateAsync("BankCashInTemplate", templateVm, TemplateType.Email),
                Subject = EmailResources.BankCashIn_Subject,
                IsHtml = true
            };

            //var stream = new MemoryStream();
            //await _srvPdfGenerator.PrintInvoice(stream, messageData.ClientId, messageData.Amount, messageData.AssetId);

            //msg.Attachments = new[]
            //{
            //    new EmailAttachment {ContentType = MediaTypeNames.Application.Pdf,
            //        FileName = "invoice.pdf", Stream = stream}
            //};

            return msg;
        }

        public async Task<EmailMessage> GenerateCashInRefundMsg(CashInRefundData refundData)
        {
            var templateVm = new BtcDepositDoneTempate
            {
                Amount = refundData.Amount,
                ExplorerUrl = string.Format(_settings.BlockChainExplorerUrl, refundData.SrcBlockchainHash),
                Year = DateTime.UtcNow.Year,
                ValidDays = refundData.ValidDays > 0 ? refundData.ValidDays : _emailGeneratorSettings.RefundTimeoutInDays
            };

            var emailMessage = new EmailMessage
            {
                Body = await _templateGenerator.GenerateAsync("BtcDepositDoneTemplate", templateVm, TemplateType.Email),
                Subject = EmailResources.Deposit_done_Subject,
                IsHtml = true
            };

            AddRefundAttachment(emailMessage, refundData.RefundTransaction);

            return emailMessage;
        }

        public async Task<EmailMessage> GenerateSwapRefundMsg(SwapRefundData refundData)
        {
            var templateVm = new SwapDoneTemplate
            {
                ExplorerUrl = string.Format(_settings.BlockChainExplorerUrl, refundData.SrcBlockchainHash),
                Year = DateTime.UtcNow.Year,
                ValidDays = refundData.ValidDays > 0 ? refundData.ValidDays : _emailGeneratorSettings.RefundTimeoutInDays
            };

            var emailMessage = new EmailMessage
            {
                Body = await _templateGenerator.GenerateAsync("SwapDoneTemplate", templateVm, TemplateType.Email),
                Subject = EmailResources.Swap_done_Subject,
                IsHtml = true
            };

            AddRefundAttachment(emailMessage, refundData.RefundTransaction);

            return emailMessage;
        }

        public async Task<EmailMessage> GenerateOrdinaryCashOutRefundMsg(OrdinaryCashOutRefundData refundData)
        {
            var templateVm = new OrdinaryCashOutDoneTemplate
            {
                Amount = refundData.Amount,
                AssetId = refundData.AssetId,
                ExplorerUrl = string.Format(_settings.BlockChainExplorerUrl, refundData.SrcBlockchainHash),
                Year = DateTime.UtcNow.Year,
                ValidDays = refundData.ValidDays > 0 ? refundData.ValidDays : _emailGeneratorSettings.RefundTimeoutInDays
            };

            var emailMessage = new EmailMessage
            {
                Body = await _templateGenerator.GenerateAsync("OCashOutDoneTemplate", templateVm, TemplateType.Email),
                Subject = EmailResources.OrdinaryCashOut_done_Subject,
                IsHtml = true
            };

            AddRefundAttachment(emailMessage, refundData.RefundTransaction);

            return emailMessage;
        }

        public async Task<EmailMessage> GenerateTransferCompletedMsg(TransferCompletedData transferCompletedData)
        {
            const int maxAccuracy = 8;

            var templateVm = new TransferTemplate
            {
                Price = transferCompletedData.Price.GetFixedAsString(maxAccuracy),
                AmountFiat = transferCompletedData.AmountFiat,
                AmountLkk = transferCompletedData.AmountLkk,
                AssetId = transferCompletedData.AssetId,
                ClientName = transferCompletedData.ClientName,
                ExplorerUrl = string.Format(_settings.BlockChainExplorerUrl, transferCompletedData.SrcBlockchainHash),
                Year = DateTime.UtcNow.Year
            };

            var emailMessage = new EmailMessage
            {
                Body = await _templateGenerator.GenerateAsync("TransferCompleteTemplate", templateVm, TemplateType.Email),
                Subject = EmailResources.TransferCompleted_Subject,
                IsHtml = true
            };

            return emailMessage;
        }

        public async Task<EmailMessage> GenerateDirectTransferCompletedMsg(DirectTransferCompletedData transferCompletedData)
        {
            var templateVm = new DirectTransferTemplate
            {
                Amount = transferCompletedData.Amount,
                AssetId = transferCompletedData.AssetId,
                ClientName = transferCompletedData.ClientName,
                ExplorerUrl = string.Format(_settings.BlockChainExplorerUrl, transferCompletedData.SrcBlockchainHash),
                Year = DateTime.UtcNow.Year
            };

            var emailMessage = new EmailMessage
            {
                Body = await _templateGenerator.GenerateAsync("DirectTransferCompleteTemplate", templateVm, TemplateType.Email),
                Subject = EmailResources.TransferCompleted_Subject,
                IsHtml = true
            };

            return emailMessage;
        }

        public async Task<EmailMessage> GeneratePrivateWalletBackupMsg(PrivateWalletBackupData privateWalletBackupData)
        {
            var backupDataJson = new
            {
                Key = privateWalletBackupData.EncodedKey,
                Question = privateWalletBackupData.SecurityQuestion
            }.ToJson();

            var rawString = backupDataJson.ToBase64();

            var qrCode = _qrCodeGenerator.GenerateGifQrCode(backupDataJson, 600);

            var qrUrl = await _backupQrRepository.SaveQrFile(privateWalletBackupData.WalletAddress, qrCode);

            var templateVm = new PrivateWalletBackupTemplate
            {
                WalletName = privateWalletBackupData.WalletName,
                WalletAddress = privateWalletBackupData.WalletAddress,
                SecurityQuestion = privateWalletBackupData.SecurityQuestion,
                QrCodeUrl = qrUrl,
                Year = DateTime.UtcNow.Year.ToString()
            };

            var emailMessage = new EmailMessage
            {
                Body =
                    await
                        _templateGenerator.GenerateAsync("PrivateWalletBackupTemplate", templateVm, TemplateType.Email),
                Subject = EmailResources.PrivateWalletBackup_Subject,
                IsHtml = true,
                Attachments = new[]
                {
                    new EmailAttachment
                    {
                        ContentType = MediaTypeNames.Text.Plain,
                        FileName = $"{privateWalletBackupData.WalletName}.txt",
                        Stream = new MemoryStream(Encoding.UTF8.GetBytes(rawString))
                    }
                }
            };

            return emailMessage;
        }

        public async Task<EmailMessage> GenerateMyLykkeCashInMsg(MyLykkeCashInData messageData)
        {
            var templateVm = new MyLykkeCashInTemplate
            {
                Amount = messageData.Amount,
                ConversionWalletAddress = messageData.ConversionWalletAddress,
                LkkAmount = messageData.LkkAmount,
                Price = messageData.Price,
                Year = DateTime.UtcNow.Year.ToString(),
                AssetId = messageData.AssetId
            };

            return new EmailMessage
            {
                Body = await _templateGenerator.GenerateAsync("MyLykkeCashInTemplate", templateVm, TemplateType.Email),
                Subject = string.Format(EmailResources.MyLykkeCashIn_Subject),
                IsHtml = true
            };
        }

        public async Task<EmailMessage> GenerateRemindPasswordMsg(RemindPasswordData messageData)
        {
            var templateVm = new RemindPasswordTemplate(messageData.PasswordHint, DateTime.UtcNow.Year);

            var emailMessage = new EmailMessage
            {
                Body = await _templateGenerator.GenerateAsync("RemindPasswordTemplate", templateVm, TemplateType.Email),
                Subject = EmailResources.RemindPassword_Subject,
                IsHtml = true
            };

            return emailMessage;
        }

        public async Task<EmailMessage> GeneratPrivateWalletAddressMsg(PrivateWalletAddressData messageData)
        {
            var templateVm = new PrivateWalletAddressTemplate
            {
                Address = messageData.Address,
                Name = messageData.Name,
                Year = DateTime.UtcNow.Year.ToString()
            };

            return new EmailMessage()
            {
                Body = await _templateGenerator.GenerateAsync("PrivateWalletAddressTemplate", templateVm, TemplateType.Email),
                Subject = EmailResources.PrivateWalletAddress_Subject,
                IsHtml = true
            };
        }

        private void AddRefundAttachment(EmailMessage emailMessage, string refundData)
        {
            emailMessage.Attachments = new[]
            {
                new EmailAttachment {ContentType = MediaTypeNames.Text.Plain,
                    FileName = "refund.txt", Stream = new MemoryStream(Encoding.UTF8.GetBytes(refundData))
                }
            };
        }

        public async Task<EmailMessage> GenerateUserRegisteredMsg(IPersonalData personalData)
        {
            var templateVm = new UserRegisteredTemplate
            {
                ContactPhone = personalData.ContactPhone,
                Country = personalData.Country,
                DateTime = personalData.Regitered,
                Email = personalData.Email,
                FullName = personalData.FullName,
                UserId = personalData.Id
            };

            return new EmailMessage
            {
                Body = await _localTemplateGenerator.GenerateAsync("UserRegisteredTemplate", templateVm, TemplateType.Email),
                Subject = EmailResources.UserRegistered_Subject,
                IsHtml = true
            };
        }

        public async Task<EmailMessage> GenerateRejectedEmailMsg()
        {
            var templateVm = new BaseTemplate
            {
                Year = DateTime.UtcNow.Year.ToString()
            };

            return new EmailMessage()
            {
                Body = await _templateGenerator.GenerateAsync("RejectedTemplate", templateVm, TemplateType.Email),
                Subject = EmailResources.Rejected_Subject,
                IsHtml = true
            };
        }

        public EmailMessage GenerateFailedTransactionMsg(string transactionId, string[] clientIds)
        {
            return new EmailMessage
            {
                Body = GetFailedTransactionBody(transactionId, clientIds),
                Subject = EmailResources.FailedTransaction_Subject,
                IsHtml = false
            };
        }

        private string GetFailedTransactionBody(string transactionId, string[] clientIds)
        {
            var sb = new StringBuilder();

            sb.AppendLine($"Transaction failed: {transactionId}.");
            sb.AppendLine("Affected clients: ");
            foreach (var id in clientIds.Distinct())
            {
                sb.AppendLine(id);
            }

            return sb.ToString();
        }
    }
}
