using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Core.Assets;

namespace Core.Bitcoin
{
    public interface IBitcoinTransactionInfo
    {
        string Hash { get; }
        DateTime Date { get; }
        int Confirmations { get; }
        string Block { get; }
        int Height { get; }
        string SenderId { get; }
        string AssetId { get; }
        int Quantity { get; }
    }

    public class BitcoinTransactionInfo : IBitcoinTransactionInfo
    {
        public string Hash { get; set; }
        public DateTime Date { get; set; }
        public int Confirmations { get; set; }
        public string Block { get; set; }
        public int Height { get; set; }
        public string SenderId { get; set; }
        public string AssetId { get; set; }
        public int Quantity { get; set; }
    }


    public interface IBalanceRecord
    {
        string AssetId { get; set; }
        double Balance { get; set; }
    }

    public interface IBalanceRecordWithBase : IBalanceRecord
    {
        string BaseAssetId { get; set; }
        double AmountInBase { get; set; }
    }

    public class BalanceRecord : IBalanceRecord
    {
        public string AssetId { get; set; }
        public double Balance { get; set; }
    }

    public class BalanceRecordWithBase : IBalanceRecordWithBase
    {
        public string AssetId { get; set; }
        public double Balance { get; set; }
        public string BaseAssetId { get; set; }
        public double AmountInBase { get; set; }
    }

    public interface ISrvBlockchainReader
    {
        Task<IEnumerable<IBlockchainTransaction>> GetTxsByAddressAsync(string address);

        Task<IBlockchainTransaction> GetByHashAsync(string hash);

        Task<string> GetColoredMultisigAsync(string multisig);

        Task<bool> IsColoredAddress(string address);

        Task<bool> IsValidAddress(string address, bool enableColored = false);

        /// <summary>
        /// Returns uncolored address (in case when uncolored address was passed just returns it)
        /// </summary>
        /// <param name="address">Colored or uncolored address</param>
        /// <returns></returns>
        Task<string> GetUncoloredAdress(string address);

        Task<IEnumerable<IBalanceRecord>> GetBalancesForAdress(string address, IAsset[] assets);

        Task<double> GetBalancesSum(IEnumerable<string> addresses, IEnumerable<IAsset> assets, IAsset asset);
    }

    public static class SrvBlockchainReaderExt
    {
        public static async Task<IBalanceRecord> GetBalanceForAdress(this ISrvBlockchainReader srvBlockchainReader, string address,
            IAsset asset)
        {
            var balance = await srvBlockchainReader.GetBalancesForAdress(address, new[] {asset});
            return balance.FirstOrDefault();
        }
    }
}