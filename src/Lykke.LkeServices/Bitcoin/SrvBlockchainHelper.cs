using System;
using System.Net;
using System.Threading.Tasks;
using Common;
using Common.HttpRemoteRequests;
using Common.Log;
using Core;
using Core.BitCoin;
using Core.Settings;
using NBitcoin;

namespace LkeServices.Bitcoin
{
    public class SrvBlockchainHelper : ISrvBlockchainHelper
    {
        private readonly IWalletCredentialsRepository _walletCredentialsRepository;
        private readonly HttpRequestClient _requestClient;
        private readonly BaseSettings _settings;
        private readonly ILog _log;
        private readonly IWalletCredentialsHistoryRepository _walletCredentialsHistoryRepository;

        public SrvBlockchainHelper(IWalletCredentialsRepository walletCredentialsRepository,
            HttpRequestClient requestClient, BaseSettings settings, ILog log, IWalletCredentialsHistoryRepository walletCredentialsHistoryRepository)
        {
            _walletCredentialsRepository = walletCredentialsRepository;
            _requestClient = requestClient;
            _settings = settings;
            _log = log;
            _walletCredentialsHistoryRepository = walletCredentialsHistoryRepository;
        }

        public async Task<IWalletCredentials> GenerateWallets(string clientId, string clientPubKeyHex, string encodedPrivateKey, string privateKey, NetworkType networkType)
        {
            var network = networkType == NetworkType.Main ? Network.Main : Network.TestNet;

            PubKey clientPubKey = new PubKey(clientPubKeyHex);
            var clientAddress = clientPubKey.GetAddress(network).ToWif();

            var wallets = await GetWalletsForPubKey(clientPubKeyHex);

            var currentWalletCreds = await _walletCredentialsRepository.GetAsync(clientId);

            IWalletCredentials walletCreds;
            if (currentWalletCreds == null)
            {
                var btcConvertionWallet = GetNewAddressAndPrivateKey(network);

                walletCreds = WalletCredentials.Create(
                    clientId, clientAddress, /*todo: remove*/privateKey, wallets.MultiSigAddress,
                    wallets.ColoredMultiSigAddress,
                    btcConvertionWallet.PrivateKey, btcConvertionWallet.Address, encodedPk: encodedPrivateKey,
                    pubKey: clientPubKeyHex);

                await _walletCredentialsRepository.SaveAsync(walletCreds);
            }
            else
            {
                walletCreds = WalletCredentials.Create(
                    clientId, clientAddress, /*todo: remove*/privateKey, wallets.MultiSigAddress,
                    wallets.ColoredMultiSigAddress, null, null, encodedPk: encodedPrivateKey,
                    pubKey: clientPubKeyHex);

                await _walletCredentialsHistoryRepository.InsertHistoryRecord(currentWalletCreds);
                await _walletCredentialsRepository.MergeAsync(walletCreds);
            }

            return walletCreds;
        }

        public async Task<ITransaction> GenerateTransferTransaction(string sourceAddress, string destAddress,
            double amount, string bcnAssetId)
        {
            var requestData = new
            {
                SourceAddress = sourceAddress,
                DestinationAddress = destAddress,
                Amount = amount,
                Asset = bcnAssetId
            };

            ITransaction result = new CreateUnsignedTransferResponse();
            try
            {
                result = (await _requestClient.Request(requestData.ToJson(),
                    _settings.WalletBackendServices.CreateUnsignedTransferPath)).DeserializeJson<CreateUnsignedTransferResponse>();
            }
            catch (Exception ex)
            {
                await _log.WriteErrorAsync("SrvBlockchainHelper", "GenerateTransferTransaction", requestData.ToJson(), ex);
            }

            return result;
        }

        public async Task BroadcastTransaction(ITransaction transaction)
        {
            var requestData = new
            {
                transaction.Id,
                ClientSignedTransaction = transaction.Hex
            };

            await _requestClient.Request(requestData.ToJson(),
                    _settings.WalletBackendServices.SignTransactionIfRequiredAndBroadcastPath);
        }

        public async Task PushPrivateKey(string privateKey, bool isP2PKH = false)
        {
            var requestData = new
            {
                PrivateKey = privateKey,
                IsP2PKH = isP2PKH
            };

            await _requestClient.Request(requestData.ToJson(), _settings.PrivateKeyServicePushMethodPath);
        }

        public bool VerifyMessage(string pubKeyAddress, string message, string signedMessage)
        {
            var address = new BitcoinPubKeyAddress(pubKeyAddress);
            try
            {
                return address.VerifyMessage(message, signedMessage);
            }
            catch
            {
                return false;
            }
        }

        public async Task<bool> IsTransactionFullyIndexed(string txHash)
        {
            try
            {
                var url = string.Format(_settings.WalletBackendServices.IsTransactionFullyIndexedUrlFormat, txHash);
                await _requestClient.GetRequest(url);
            }
            catch (WebException)
            {
                await
                    _log.WriteWarningAsync("SrvBlockchainHelper", "IsTransactionFullyIndexed", txHash,
                        $"Tx {txHash} was not fully indexed");
                return false;
            }

            return true;
        }

        #region Tools

        private async Task<GetWalletResponse> GetWalletsForPubKey(string pubKeyHex)
        {
            GetWalletResponse result;
            try
            {
                result = (await _requestClient.Request($"={pubKeyHex}",
                    _settings.WalletBackendServices.GetWalletPath, "application/x-www-form-urlencoded")).DeserializeJson<GetWalletResponse>();
            }
            catch (Exception ex)
            {
                await _log.WriteErrorAsync("SrvBlockchainHelper", "GenerateTransferTransaction", pubKeyHex, ex);
                throw;
            }

            return result;
        }

        class WalletKeyAndAddress
        {
            public string PrivateKey { get; set; }
            public string Address { get; set; }
        }

        private WalletKeyAndAddress GetNewAddressAndPrivateKey(Network network)
        {
            Key key = new Key();
            BitcoinSecret secret = new BitcoinSecret(key, network);

            var walletAddress = secret.GetAddress().ToWif();
            var walletPrivateKey = secret.PrivateKey.GetWif(network).ToWif();

            return new WalletKeyAndAddress
            {
                Address = walletAddress,
                PrivateKey = walletPrivateKey
            };
        }

        #endregion

        #region WalletBackend response models

        internal class CreateUnsignedTransferResponse : ITransaction
        {
            public string Id { get; set; }
            public string Hex => TransactionHex;
            public string TransactionHex { get; set; }
        }

        internal class GetWalletResponse
        {
            public string MultiSigAddress { get; set; }
            public string ColoredMultiSigAddress { get; set; }
        }

        #endregion
    }
}
