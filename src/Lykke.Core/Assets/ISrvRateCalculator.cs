using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Core.Bitcoin;
using Core.Exchange;
using Core.Feed;

namespace Core.Assets
{
    public interface ISrvRateCalculator
    {
        Task<double> GetRate(string neededAssetId, IAssetPair assetPair);
        Task<AmountAndPrice> GetConvertedAmountAndPriceAsync(string fromAssetId, string toAssetId, double fromAmount);
        Task<IEnumerable<IBalanceRecordWithBase>> FillBaseAssetData(IEnumerable<IBalanceRecord> balanceRecords, string baseAssetId);

        Task<IEnumerable<Tuple<string, double>>> GetAmountInBase(
            IEnumerable<Tuple<string, double>> balanceRecords, string toAssetId);

        Task<double> GetAmountInBase(string assetFrom, double amount, string assetTo,
            MarketProfile marketProfile = null);

        Task<IEnumerable<ConversionResult>> GetMarketAmountInBase(IEnumerable<AssetWithAmount> assetsFrom, string assetIdTo, OrderAction orderAction);
    }

    #region Models

    public class AmountAndPrice
    {
        public AmountAndPrice(double amount, double price, double invertedPrice)
        {
            Amount = amount;
            Price = price;
            InvertedPrice = invertedPrice;
        }

        public double Amount { get; set; }

        public double Price { get; set; }

        public double InvertedPrice { get; set; }
    }

    public class AssetWithAmount
    {
        public string AssetId { get; set; }
        public double Amount { get; set; }
    }

    public class ConversionResult
    {
        public AssetWithAmount From { get; set; }
        public AssetWithAmount To { get; set; }
        public double Price { get; set; }

        /// <summary>
        /// Price from order book according to volume
        /// </summary>
        public double VolumePrice { get; set; }

        public string Result => _result.ToString();

        private OperationResult _result;

        public void SetResult(OperationResult result)
        {
            _result = result;
        }
    }

    public enum OperationResult
    {
        Unknown,
        Ok,
        InvalidInputParameters,
        NoLiquidity
    }

    #endregion
}