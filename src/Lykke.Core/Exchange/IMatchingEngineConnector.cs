using System.Threading.Tasks;

namespace Core.Exchange
{
    public class CashInOutResponse
    {
        public string RecordId { get; set; }
        public string CorrelationId { get; set; }
    }

    public interface IMatchingEngineConnector
    {
        Task<string> HandleMarketOrderAsync(string clientId, string assetPairId, OrderAction orderAction, double volume, bool straight);
        Task HandleLimitOrderAsync(string clientId, string assetPairId, OrderAction orderAction, double volume, double price);

        Task<CashInOutResponse> CashInOutBalanceAsync(string clientId, string assetId, double balanceDelta, bool sendToBlockchain, string correlationId);

        Task UpdateBalanceAsync(string clientId, string assetId, double value);

        Task CancelLimitOrderAsync(int orderId);

        /// <summary>
        /// Update Wallet Credentials cache in ME
        /// </summary>
        /// <param name="clientId"></param>
        /// <returns>True if update was successful</returns>
        Task<bool> UpdateWalletCredsForClient(string clientId);
    }
}
