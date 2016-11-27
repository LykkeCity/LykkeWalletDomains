using System.Threading.Tasks;

namespace Core.BitCoin
{
    public interface IBitCoinCommandProducer
    {
        Task<string> ProduceGenerateNewWalletCommand(string transactionId);

        Task<string> ProduceCashInCommand(string transactionId, string multisigAddress, double amount, string currency);

        Task<string> ObsoleteProduceOrdinaryCashOutCommand(string transactionId, string multisigAddress, double amount, string currency, string privateKey, string addresToCashOut);

        Task<string> ProduceOrdinaryCashOutCommand(string transactionId, string multisigAddress, double amount, string currency, string addresToCashOut);

        Task<string> ProduceGenerateFeeOutputsAsync(string transactionId, string walletAddress, string privateKey, double feeAmount, double count);

        Task<string> ProduceGenerateTransferAsync(string transactionId, string sourceAddress, string sourceKey, string destinationAddress, double amount, string assetId);

        Task<string> ProduceGenerateRefundingTransactionAsync(string transactionId, string multisigAddress, string pubKey, string refundAddress, int timeoutInMinutes);

        Task ProduceGetFeeOutputsStatusAsync(string transactionId);

        Task<string> UpdateAssets(string transactionId, UpdateAssetItem[] items);

        Task<string> TransferAllAssetsToAddress(string transactionId, string srcAddres, string srcPrivateKey,
            string destAddress);
    }

    public class UpdateAssetItem
    {
        public string AssetId { get; set; }
        public string AssetAddress { get; set; }
        public string PrivateKey { get; set; }
        public string Name { get; set; }
        public string DefinitionUrl { get; set; }
        public int Divisibility { get; set; }
    }
}
