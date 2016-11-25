using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Core.Assets.AssetGroup;
using Core.AuditLog;
using Core.BackgroundWorker;
using Core.BackOffice;
using Core.BitCoin;
using Core.Clients;
using Core.EventLogs;
using Core.Kyc;
using Core.Messages;
using Core.Settings;

namespace LkeServices.Kyc
{
    public class SrvKycManager
    {
        private readonly IKycDocumentsRepository _kycDocumentsRepository;
        private readonly IKycDocumentsScansRepository _kycDocumentsScansRepository;
        private readonly IKycRepository _kycRepository;
        private readonly IMenuBadgesRepository _menuBadgesRepository;
        private readonly IPersonalDataRepository _personalDataRepository;
        private readonly IClientAccountsRepository _clientAccountsRepository;
        private readonly IAuditLogRepository _auditLogRepository;
        private readonly IRegistrationLogs _registrationLogs;
        private readonly IClientSettingsRepository _clientSettingsRepository;
        private readonly ISrvBlockchainHelper _srvBlockchainHelper;
        private readonly BaseSettings _baseSettings;
        private readonly IAppGlobalSettingsRepositry _appGlobalSettingsRepositry;
        private readonly IAssetGroupRepository _assetGroupRepository;
        private readonly ISrvEmailsFacade _srvEmailsFacade;
        private readonly IBackgroundWorkRequestProducer _backgroundWorkRequestProducer;

        public SrvKycManager(IKycDocumentsRepository kycDocumentsRepository, IKycDocumentsScansRepository kycDocumentsScansRepository,
            IKycRepository kycRepository, IMenuBadgesRepository menuBadgesRepository,
            IPersonalDataRepository personalDataRepository, IClientAccountsRepository clientAccountsRepository,
            IAuditLogRepository auditLogRepository, IRegistrationLogs registrationLogs, IClientSettingsRepository clientSettingsRepository,
            ISrvBlockchainHelper srvBlockchainHelper, BaseSettings baseSettings,
            IAppGlobalSettingsRepositry appGlobalSettingsRepositry, IAssetGroupRepository assetGroupRepository,
            ISrvEmailsFacade srvEmailsFacade, IBackgroundWorkRequestProducer backgroundWorkRequestProducer)
        {
            _kycDocumentsRepository = kycDocumentsRepository;
            _kycDocumentsScansRepository = kycDocumentsScansRepository;
            _kycRepository = kycRepository;
            _menuBadgesRepository = menuBadgesRepository;
            _personalDataRepository = personalDataRepository;
            _clientAccountsRepository = clientAccountsRepository;
            _auditLogRepository = auditLogRepository;
            _registrationLogs = registrationLogs;
            _clientSettingsRepository = clientSettingsRepository;
            _srvBlockchainHelper = srvBlockchainHelper;
            _baseSettings = baseSettings;
            _appGlobalSettingsRepositry = appGlobalSettingsRepositry;
            _assetGroupRepository = assetGroupRepository;
            _srvEmailsFacade = srvEmailsFacade;
            _backgroundWorkRequestProducer = backgroundWorkRequestProducer;
        }

        #region Documents
        public async Task<string> UploadDocument(string clientId, string type, string fileName, string mime, byte[] data,
            string changer)
        {
            var documentBeforeTask = _kycDocumentsRepository.GetAsync(clientId);
            var kycDocument = await _kycDocumentsRepository.AddAsync(KycDocument.Create(clientId, type, mime, fileName));
            await _kycDocumentsScansRepository.AddDocument(kycDocument.DocumentId, data);

            await UpdateKycProfileSettings(clientId);

            var documentBefore = (await documentBeforeTask)?
                        .OrderByDescending(x => x.DateTime)
                        .FirstOrDefault(x => x.Type == type);

            await _auditLogRepository.AddAuditRecordAsync(clientId, documentBefore, kycDocument,
                AuditRecordType.KycDocument, changer);

            return kycDocument.DocumentId;
        }

        public async Task DeclineDocumentAsync(string clientId, string documentId, string changer, string comment)
        {
            var docBefore = await _kycDocumentsRepository.GetAsync(clientId, documentId);

            var doc = await
                _kycDocumentsRepository.SetStateAndComment(clientId, documentId, comment,
                    DocumentStates.Declined);

            await UpdateKycProfileSettings(clientId);

            await _auditLogRepository.AddAuditRecordAsync(clientId, docBefore, doc, AuditRecordType.KycDocument, changer);
        }

        private async Task UpdateKycProfileSettings(string clientId)
        {
            var documents = (await _kycDocumentsRepository.GetOneEachTypeLatestAsync(clientId)).ToArray();

            var settings = await _clientSettingsRepository.GetSettings<KycProfileSettings>(clientId);

            settings.ShowIdCard = !documents.HasType(KycDocumentTypes.IdCard);
            settings.ShowIdProofOfAddress = !documents.HasType(KycDocumentTypes.ProofOfAddress);
            settings.ShowSelfie = !documents.HasType(KycDocumentTypes.Selfie);

            await _clientSettingsRepository.SetSettings(clientId, settings);
        }
        #endregion

        #region KYC Status
        private async Task UpdateKycBadge()
        {
            var count = (await _kycRepository.GetClientsByStatus(KycStatus.Pending)).Count();
            await _menuBadgesRepository.SaveBadgeAsync(MenuBadges.Kyc, count.ToString());
        }

        public async Task<bool> ChangeKycStatus(string clientId, KycStatus kycStatus, string changer)
        {
            var currentStatus = await _kycRepository.GetKycStatusAsync(clientId);

            if (currentStatus != kycStatus)
            {
                await _kycRepository.SetStatusAsync(clientId, kycStatus);
                await _auditLogRepository.AddAuditRecordAsync(clientId, currentStatus, kycStatus,
                    AuditRecordType.KycStatus, changer);
                await UpdateKycBadge();
                return true;
            }

            return false;
        }


        public async Task<IEnumerable<IPersonalData>> GetAccountsToCheck()
        {
            var ids = (await _kycRepository.GetClientsByStatus(KycStatus.Pending)).ToList();
            return (await _personalDataRepository.GetAsync(ids)).OrderBy(x => ids.IndexOf(x.Id)).ToList(); ;
        }
        #endregion

        #region Clients
        public async Task<IClientAccount> RegisterClientAsync(string email, string fullname, string phone, string password,
            string hint, string clientInfo, string ip, string changer, double? iosVersion = null)
        {
            IClientAccount clientAccount = ClientAccount.Create(email, phone);

            clientAccount = await _clientAccountsRepository.RegisterAsync(clientAccount, password);

            await _clientSettingsRepository.SetSettings(clientAccount.Id, new HashedPwdSettings { IsPwdHashed = true });

            var personalData = FullPersonalData.Create(clientAccount, fullname, hint);
            await _personalDataRepository.SaveAsync(personalData);

            await SetDefaultAssetGroups(clientAccount.Id, iosVersion);

            var logEvent = RegistrationLogEvent.Create(clientAccount.Id, email, fullname, phone, clientInfo, ip);
            await _registrationLogs.RegisterEventAsync(logEvent);

            await _auditLogRepository.AddAuditRecordAsync(clientAccount.Id, null, personalData,
                AuditRecordType.PersonalData, changer);

            await _srvEmailsFacade.SendWelcomeEmail(clientAccount.Email, clientAccount.Id);

            await
                _backgroundWorkRequestProducer.ProduceRequest(WorkType.SetGeolocation,
                    new SetGeolocationContext(clientAccount.Id, ip));

            await
                _backgroundWorkRequestProducer.ProduceRequest(WorkType.SetReferralCode,
                    new SetReferralCodeContext(clientAccount.Id, ip));

            await
                _backgroundWorkRequestProducer.ProduceRequest(WorkType.SetAntiFraudRecord,
                    new SetAntiFraudContext(clientAccount.Id, ip, email, fullname, phone));

            return clientAccount;

        }

        private async Task SetDefaultAssetGroups(string clientId, double? iosVersion = null)
        {
            var globalSettings = await _appGlobalSettingsRepositry.GetAsync();

            if (globalSettings.IsOnReview && iosVersion != null
                && iosVersion.Value >= globalSettings.MinVersionOnReview)
            {
                await _assetGroupRepository.AddClientToGroup(clientId, globalSettings.ReviewIosGroup);
                await _clientSettingsRepository.SetSettings(clientId, new MyLykkeSettings { MyLykkeEnabled = false });
            }
            else if (!string.IsNullOrEmpty(globalSettings.DefaultIosAssetGroup))
                await _assetGroupRepository.AddClientToGroup(clientId, globalSettings.DefaultIosAssetGroup);

            if (!string.IsNullOrEmpty(globalSettings.DefaultAssetGroupForOther))
                await _assetGroupRepository.AddClientToGroup(clientId, globalSettings.DefaultAssetGroupForOther);
        }

        public async Task UpdatePersonalDataAsync(IPersonalData personalData, string changer)
        {
            var dataBefore = await _personalDataRepository.GetAsync(personalData.Id);
            await _personalDataRepository.UpdateAsync(personalData);
            var dataAfter = await _personalDataRepository.GetAsync(personalData.Id);

            await _auditLogRepository.AddAuditRecordAsync(personalData.Id, dataBefore, dataAfter,
                AuditRecordType.PersonalData, changer);
        }

        public async Task ChangePhoneAsync(string clientId, string phoneNumber, string changer)
        {
            var dataBefore = await _personalDataRepository.GetAsync(clientId);
            await _clientAccountsRepository.ChangePhoneAsync(clientId, phoneNumber);
            await _personalDataRepository.ChangeContactPhoneAsync(clientId, phoneNumber);
            var dataAfter = await _personalDataRepository.GetAsync(clientId);

            await _auditLogRepository.AddAuditRecordAsync(clientId, dataBefore, dataAfter, AuditRecordType.PersonalData, changer);
        }

        public async Task SplitFullName(string clientId, string fullName, string changer)
        {
            var fullNameSplit = fullName.Split(" ".ToCharArray());

            var firstName = (fullNameSplit[0] ?? "").Trim();
            var lastName = fullName.Substring(firstName.Length, fullName.Length - firstName.Length).Trim(); // all after first name

            await Task.WhenAll(ChangeFirstNameAsync(clientId, firstName, changer),
                ChangeLastNameAsync(clientId, lastName, changer));
        }

        public async Task ChangeFullNameAsync(string clientId, string fullName, string changer)
        {
            var dataBefore = await _personalDataRepository.GetAsync(clientId);
            await _personalDataRepository.ChangeFullNameAsync(clientId, fullName);
            var dataAfter = await _personalDataRepository.GetAsync(clientId);

            await _auditLogRepository.AddAuditRecordAsync(clientId, dataBefore, dataAfter, AuditRecordType.PersonalData, changer);
        }

        public async Task ChangeFirstNameAsync(string clientId, string firstName, string changer)
        {
            var dataBefore = await _personalDataRepository.GetAsync(clientId);
            await _personalDataRepository.ChangeFirstNameAsync(clientId, firstName);
            var dataAfter = await _personalDataRepository.GetAsync(clientId);

            await _auditLogRepository.AddAuditRecordAsync(clientId, dataBefore, dataAfter, AuditRecordType.PersonalData, changer);
        }

        public async Task ChangeLastNameAsync(string clientId, string lastName, string changer)
        {
            var dataBefore = await _personalDataRepository.GetAsync(clientId);
            await _personalDataRepository.ChangeLastNameAsync(clientId, lastName);
            var dataAfter = await _personalDataRepository.GetAsync(clientId);

            await _auditLogRepository.AddAuditRecordAsync(clientId, dataBefore, dataAfter, AuditRecordType.PersonalData, changer);
        }
        public async Task ChangeZipAsync(string clientId, string zip, string changer)
        {
            var dataBefore = await _personalDataRepository.GetAsync(clientId);
            await _personalDataRepository.ChangeZipAsync(clientId, zip);
            var dataAfter = await _personalDataRepository.GetAsync(clientId);

            await _auditLogRepository.AddAuditRecordAsync(clientId, dataBefore, dataAfter, AuditRecordType.PersonalData, changer);
        }
        public async Task ChangeCityAsync(string clientId, string city, string changer)
        {
            var dataBefore = await _personalDataRepository.GetAsync(clientId);
            await _personalDataRepository.ChangeCityAsync(clientId, city);
            var dataAfter = await _personalDataRepository.GetAsync(clientId);

            await _auditLogRepository.AddAuditRecordAsync(clientId, dataBefore, dataAfter, AuditRecordType.PersonalData, changer);
        }

        public async Task ChangeAddressAsync(string clientId, string address, string changer)
        {
            var dataBefore = await _personalDataRepository.GetAsync(clientId);
            await _personalDataRepository.ChangeAddressAsync(clientId, address);
            var dataAfter = await _personalDataRepository.GetAsync(clientId);

            await _auditLogRepository.AddAuditRecordAsync(clientId, dataBefore, dataAfter, AuditRecordType.PersonalData, changer);
        }

        public async Task ChangeCountryAsync(string clientId, string country, string changer)
        {
            var dataBefore = await _personalDataRepository.GetAsync(clientId);
            await _personalDataRepository.ChangeCountryAsync(clientId, country);
            var dataAfter = await _personalDataRepository.GetAsync(clientId);

            await _auditLogRepository.AddAuditRecordAsync(clientId, dataBefore, dataAfter, AuditRecordType.PersonalData, changer);
        }
        #endregion
    }

    public static class RecordChanger
    {
        public const string Client = "Client";
    }
}
