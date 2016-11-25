using System;
using System.Linq;
using System.Threading.Tasks;
using Common;
using Common.Log;
using Core;
using Core.Accounts;
using Core.Assets;
using Core.Bitcoin;
using Core.BitCoin;
using Core.EventLogs;
using Core.Exchange;
using Core.Settings;

namespace LkeServices.Bitcoin
{
    public class SrvBitcoinCommandProducer
    {
        private readonly IBitCoinCommandProducer _bitCoinCommandProducer;
        private readonly IBitCoinTransactionsRepository _bitCoinTransactionsRepository;
        private readonly IWalletCredentialsRepository _walletCredentialsRepository;
        private readonly IAssetsRepository _assetsRepository;
        private readonly ICashOperationsRepository _cashOperationsRepository;
        private readonly IBalancePendingRepository _balancePendingRepository;
        private readonly ILog _log;
        private readonly IMatchingEngineConnector _matchingEngineConnector;
        private readonly ITransferEventsRepository _transferEventsRepository;
        private readonly ISrvBlockchainHelper _srvBlockchainHelper;
        private readonly IWalletsRepository _walletsRepository;
        private readonly ISrvBlockchainReader _srvBlockchainReader;
        private readonly CachedDataDictionary<string, IAsset> _assetsDict;
        private readonly BaseSettings _baseSettings;


        public SrvBitcoinCommandProducer(IBitCoinCommandProducer bitCoinCommandProducer, IBitCoinTransactionsRepository bitCoinTransactionsRepository,
            IWalletCredentialsRepository walletCredentialsRepository, IAssetsRepository assetsRepository, ICashOperationsRepository cashOperationsRepository,
            IBalancePendingRepository balancePendingRepository, ILog log, BaseSettings baseSettings, IMatchingEngineConnector matchingEngineConnector,
            ITransferEventsRepository transferEventsRepository, ISrvBlockchainHelper srvBlockchainHelper, IWalletsRepository walletsRepository,
            ISrvBlockchainReader srvBlockchainReader, CachedDataDictionary<string, IAsset> assetsDict)
        {
            _bitCoinCommandProducer = bitCoinCommandProducer;
            _bitCoinTransactionsRepository = bitCoinTransactionsRepository;
            _walletCredentialsRepository = walletCredentialsRepository;
            _assetsRepository = assetsRepository;
            _cashOperationsRepository = cashOperationsRepository;
            _balancePendingRepository = balancePendingRepository;
            _log = log;
            _matchingEngineConnector = matchingEngineConnector;
            _transferEventsRepository = transferEventsRepository;
            _srvBlockchainHelper = srvBlockchainHelper;
            _walletsRepository = walletsRepository;
            _srvBlockchainReader = srvBlockchainReader;
            _assetsDict = assetsDict;
            _baseSettings = baseSettings;
        }

        public async Task GenerateClientWalletAsync(string clientId)
        {

            var walletData = await _walletCredentialsRepository.GetAsync(clientId);

            if (walletData != null)
                return;

            var transactionId = Guid.NewGuid().ToString("N");
            try
            {
                await _bitCoinTransactionsRepository.CreateAsync(transactionId, null, GenerateWalletContextData.Create(clientId));
                var queueMsg = await _bitCoinCommandProducer.ProduceGenerateNewWalletCommand(transactionId);
                await _bitCoinTransactionsRepository.UpdateRequestAsync(transactionId, queueMsg);
            }
            catch (Exception ex)
            {
                await
                    _log.WriteErrorAsync("SrvBitcoinCommandProduce", "GenerateClientWalletAsync",
                        $"ClientId:{clientId}; TransactionId:{transactionId}", ex);
            }
        }

        public async Task GenerateCashInAsync(string clientId, string assetId, double amount)
        {
            var walletData = await _walletCredentialsRepository.GetAsync(clientId);
            var asset = await _assetsRepository.GetAssetAsync(assetId);
            var transactionId = Guid.NewGuid().ToString("N");

            var pendingRecordId = await _balancePendingRepository.CreateAsync(clientId, assetId, amount);
            var contextData = new CashInContextData(clientId, pendingRecordId);

            await _bitCoinTransactionsRepository.CreateAsync(transactionId, BitCoinCommands.CashIn, contextData);

            var request = await
                _bitCoinCommandProducer.ProduceCashInCommand(transactionId, walletData.MultiSig, amount,
                    asset.BlockChainId);

            await _bitCoinTransactionsRepository.UpdateRequestAsync(transactionId, request);
        }


        public async Task GenerateOrdinaryCashOut(string clientId, string address, double amount, string assetId, string privateKey)
        {
            var asset = await _assetsRepository.GetAssetAsync(assetId);
            if (asset == null)
                return;

            CashOutContextData contextData = null;
            try
            {
                var walletCredentials = await _walletCredentialsRepository.GetAsync(clientId);

                var cashOpId = await _cashOperationsRepository.RegisterAsync(new CashInOutOperation
                {
                    Id = Guid.NewGuid().ToString("N"),
                    ClientId = clientId,
                    Multisig = walletCredentials.MultiSig,
                    AssetId = assetId,
                    Amount = -Math.Abs(amount),
                    DateTime = DateTime.UtcNow,
                    AddressFrom = walletCredentials.MultiSig,
                    AddressTo = address
                });

                var transactionId = Guid.NewGuid().ToString("N");
                contextData = CashOutContextData.Create(clientId, assetId, address, amount, cashOpId);

                await
                    _bitCoinTransactionsRepository.CreateAsync(transactionId, BitCoinCommands.OrdinaryCashOut,
                        contextData);

                await _matchingEngineConnector.CashInOutBalanceAsync(contextData.ClientId, contextData.AssetId,
                -Math.Abs(amount), false, cashOpId);

                string request;
                if (_baseSettings.UsePushPrivateKeyService)
                {
                    await _srvBlockchainHelper.PushPrivateKey(privateKey);
                    
                    request =
                        await _bitCoinCommandProducer.ProduceOrdinaryCashOutCommand(transactionId, walletCredentials.MultiSig, amount,
                            asset.BlockChainId, address);
                }
                else //ToDo: Temporary. Remove, when will be ready in ME
                {
                    request =
                        await _bitCoinCommandProducer.ObsoleteProduceOrdinaryCashOutCommand(transactionId, walletCredentials.MultiSig, amount,
                            asset.BlockChainId, privateKey, address);
                }

                await _bitCoinTransactionsRepository.UpdateRequestAsync(transactionId, request);
            }
            catch (Exception ex)
            {
                await _log.WriteErrorAsync("SrvBitcoinCommandProducer", "OrdinaryCashOut", contextData?.ToJson(), ex);
            }
        }

        public async Task GenerateRefundAsync(string clientId, string refundAddress, string operationType, string srcBlockchainHash,
            int timeoutInMinutes, double? amount = null, string assetId = null)
        {
            var walletData = await _walletCredentialsRepository.GetAsync(clientId);

            var transactionId = Guid.NewGuid().ToString("N");
            try
            {
                await _bitCoinTransactionsRepository.CreateAsync(transactionId, null, new RefundContextData
                {
                    ClientId = clientId,
                    OperationType = operationType,
                    Amount = amount,
                    SrcBlockchainHash = srcBlockchainHash,
                    AssetId = assetId
                });

                var queueMsg = await _bitCoinCommandProducer.ProduceGenerateRefundingTransactionAsync(transactionId, walletData.MultiSig, walletData.PublicKey,
                    refundAddress, timeoutInMinutes);
                await _bitCoinTransactionsRepository.UpdateRequestAsync(transactionId, queueMsg);
            }
            catch (Exception ex)
            {
                await
                    _log.WriteErrorAsync("SrvBitcoinCommandProduce", "GenerateRefundAsync",
                        $"ClientId:{clientId}; TransactionId:{transactionId}", ex);
            }
        }

        public async Task GenerateFeeOutputs(string walletAddress, string privateKey, double feeAmount, double count)
        {
            var transactionId = Guid.NewGuid().ToString("N");
            try
            {
                await _bitCoinTransactionsRepository.CreateAsync(transactionId, null, string.Empty);

                if (_baseSettings.UsePushPrivateKeyService)
                {
                    await _srvBlockchainHelper.PushPrivateKey(privateKey, true);
                    privateKey = null;
                }

                var queueMsg = await _bitCoinCommandProducer.ProduceGenerateFeeOutputsAsync(transactionId, walletAddress,
                    privateKey, feeAmount, count);

                await _bitCoinTransactionsRepository.UpdateRequestAsync(transactionId, queueMsg);
            }
            catch (Exception ex)
            {
                await
                    _log.WriteErrorAsync("SrvBitcoinCommandProduce", "GenerateFeeOutputs",
                        $"Wallet:{walletAddress}; FeeAmount:{feeAmount}; Count:{count}", ex);
            }
        }

        public async Task TransferFromOrdinaryWalletWithNotification(string clientId, string sourceAddress, string sourceKey,
            double amount, string assetId)
        {
            var additionalActionsDest = new TransferContextData.AdditionalActions
            {
                SendTransferEmail = new TransferContextData.EmailAction(assetId, amount),
                PushNotification = new TransferContextData.PushNotification(assetId, amount)
            };

            await
                GenerateTransferFromOrdinaryWallet(clientId, sourceAddress, sourceKey, amount, assetId,
                    additionalActionsDest);
        }

        public async Task TransferConvertedBetweenClientsWithNotification(string destClientId, string sourceClientId,
            double amount, string assetId, double price, double amountFrom, string fromAssetId, bool fromBtcConvWallet = false)
        {
            var additionalActionsDest = new TransferContextData.AdditionalActions
            {
                CashInConvertedOkEmail = new TransferContextData.ConvertedOkEmailAction(fromAssetId, price, amountFrom, amount),
                PushNotification = new TransferContextData.PushNotification(assetId, amount)
            };

            await GenerateTransfer(destClientId, sourceClientId, amount, assetId, fromBtcConvWallet, additionalActionsDest);
        }

        public async Task<string> TransferBetweenClientsWithNotification(string destClientId, string sourceClientId,
            double amount, string assetId)
        {
            var additionalActionsDest = new TransferContextData.AdditionalActions
            {
                SendTransferEmail = new TransferContextData.EmailAction(assetId, amount),
                PushNotification = new TransferContextData.PushNotification(assetId, amount)
            };

            return await GenerateTransfer(destClientId, sourceClientId, amount, assetId, false, additionalActionsDest);
        }

        public async Task<string> GenerateTransfer(string destClientId, string sourceClientId, double amount, string assetId,
            bool fromBtcConvWallet = false,
            TransferContextData.AdditionalActions additionalActionsDest = null,
            TransferContextData.AdditionalActions additionalActionsSrc = null)
        {
            var asset = await _assetsRepository.GetAssetAsync(assetId);
            if (asset == null)
                throw new ArgumentNullException(nameof(assetId));

            amount = Math.Abs(amount);

            var transactionId = Guid.NewGuid().ToString("N");

            var destClientCredentials = await _walletCredentialsRepository.GetAsync(destClientId);
            var sourceWalletCred = await _walletCredentialsRepository.GetAsync(sourceClientId);

            var destTransfer =
                await
                    _transferEventsRepository.RegisterAsync(TransferEvent.CreateNew(destClientId, destClientCredentials.MultiSig, null, assetId, amount, transactionId,
                    destClientCredentials.Address, destClientCredentials.MultiSig));

            var sourceTransfer =
                await
                    _transferEventsRepository.RegisterAsync(TransferEvent.CreateNew(sourceClientId,
                        fromBtcConvWallet ? sourceWalletCred.BtcConvertionWalletAddress : sourceWalletCred.MultiSig,
                        null, assetId, -amount, transactionId,
                        sourceWalletCred.Address, sourceWalletCred.MultiSig));

            var contextData = TransferContextData.Create(new TransferContextData.TransferModel
            {
                ClientId = destClientId,
                OperationId = destTransfer.Id,
                Actions = additionalActionsDest
            }, new TransferContextData.TransferModel
            {
                ClientId = sourceClientId,
                OperationId = sourceTransfer.Id,
                Actions = additionalActionsSrc
            });


            var pk = fromBtcConvWallet
                ? sourceWalletCred.BtcConvertionWalletPrivateKey
                : sourceWalletCred.PrivateKey;

            if (_baseSettings.UsePushPrivateKeyService)
            {
                await _srvBlockchainHelper.PushPrivateKey(pk, fromBtcConvWallet);
            }

            var queueMsg = await
                _bitCoinCommandProducer.ProduceGenerateTransferAsync(transactionId,
                    fromBtcConvWallet ? sourceWalletCred.BtcConvertionWalletAddress : sourceWalletCred.MultiSig,
                    _baseSettings.UsePushPrivateKeyService? null : pk, destClientCredentials.MultiSig, amount, asset.BlockChainId);

            await _bitCoinTransactionsRepository.CreateAsync(transactionId, queueMsg, contextData);

            return destTransfer.Id;
        }

        public async Task GenerateTransferFromOrdinaryWallet(string clientId, string sourceAddress, string sourceKey, double amount, string assetId,
            TransferContextData.AdditionalActions additionalActionsDest = null)
        {
            var asset = await _assetsRepository.GetAssetAsync(assetId);
            if (asset == null)
                return;

            amount = Math.Abs(amount);

            var transactionId = Guid.NewGuid().ToString("N");

            var clientCreds = await _walletCredentialsRepository.GetAsync(clientId);

            var transfer =
                await
                    _transferEventsRepository.RegisterAsync(TransferEvent.CreateNew(clientId, clientCreds.MultiSig, null, assetId, amount, transactionId,
                        sourceAddress, clientCreds.MultiSig));

            var contextData = TransferContextData.Create(new TransferContextData.TransferModel
            {
                ClientId = clientId,
                OperationId = transfer.Id,
                Actions = additionalActionsDest
            });

            if (_baseSettings.UsePushPrivateKeyService)
            {
                await _srvBlockchainHelper.PushPrivateKey(sourceKey, true);
                sourceKey = null;
            }

            var queueMsg = await
                _bitCoinCommandProducer.ProduceGenerateTransferAsync(transactionId, sourceAddress, sourceKey,
                    clientCreds.MultiSig, amount, asset.BlockChainId);

            await _bitCoinTransactionsRepository.CreateAsync(transactionId, queueMsg, contextData);
        }

        public async Task UpdateAssets(UpdateAssetItem[] items)
        {
            var transactionId = Guid.NewGuid().ToString("N");
            await _bitCoinTransactionsRepository.CreateAsync(transactionId, null, string.Empty);
            var queueMsg = await _bitCoinCommandProducer.UpdateAssets(transactionId, items);
            await _bitCoinTransactionsRepository.UpdateRequestAsync(transactionId, queueMsg);
        }

        public async Task TransferAllAssetsToAddress(string clientId, string srcAddress, string srcPrivateKey, string destAddress)
        {
            var transactionId = Guid.NewGuid().ToString("N");
            var transfer =
                await
                    _transferEventsRepository.RegisterAsync(TransferEvent.CreateNewTransferAll(clientId, transactionId, srcAddress));

            var contextData = TransferContextData.Create(new TransferContextData.TransferModel
            {
                ClientId = clientId,
                OperationId = transfer.Id
            });

            await _bitCoinTransactionsRepository.CreateAsync(transactionId, null, contextData);

            if (_baseSettings.UsePushPrivateKeyService)
            {
                await _srvBlockchainHelper.PushPrivateKey(srcPrivateKey);
                srcPrivateKey = null;
            }

            var assets = (await _assetsDict.Values()).ToArray();
            bool needToTransferAssets = (await _srvBlockchainReader.GetBalancesForAdress(srcAddress, assets)).Any(x => x.Balance > 0);     

            if (needToTransferAssets)
            {
                var queueMsg = await _bitCoinCommandProducer.TransferAllAssetsToAddress(transactionId, srcAddress, srcPrivateKey, destAddress);
                await _bitCoinTransactionsRepository.UpdateRequestAsync(transactionId, queueMsg);
            }
        }

    }
}
