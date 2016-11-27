using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Common;
using Core.Assets;
using Core.Bitcoin;
using Core.Exchange;
using Core.Feed;

namespace LkeServices.Assets
{
    public class SrvRateCalculator : ISrvRateCalculator
    {
        private readonly CachedDataDictionary<string, IAsset> _assetsDict;
        private readonly CachedDataDictionary<string, IAssetPair> _assetPairsDict;
        private readonly IAssetPairBestPriceRepository _bestPriceRepository;
        private readonly IActiveLimitOrdersRepository _activeLimitOrdersRepository;

        public SrvRateCalculator(CachedDataDictionary<string, IAsset> assetsDict,
            CachedDataDictionary<string, IAssetPair> assetPairsDict,
            IAssetPairBestPriceRepository bestPriceRepository,
            IActiveLimitOrdersRepository activeLimitOrdersRepository)
        {
            _assetsDict = assetsDict;
            _assetPairsDict = assetPairsDict;
            _bestPriceRepository = bestPriceRepository;
            _activeLimitOrdersRepository = activeLimitOrdersRepository;
        }

        public async Task<double> GetRate(string neededAssetId, IAssetPair assetPair)
        {
            var rates = await _bestPriceRepository.GetAsync(assetPair.Id);
            return GetRate(neededAssetId, assetPair, rates.Ask);
        }

        public double GetRate(string neededAssetId, IAssetPair assetPair, double price)
        {
            var inverted = assetPair.IsInverted(neededAssetId);
            int accuracy = inverted ? assetPair.Accuracy : assetPair.InvertedAccuracy;
            var rate = inverted ? price : 1 / price;

            return rate.TruncateDecimalPlaces(accuracy);
        }


        public async Task<AmountAndPrice> GetConvertedAmountAndPriceAsync(string fromAssetId, string toAssetId, double fromAmount)
        {
            if (string.IsNullOrEmpty(fromAssetId))
                throw new ArgumentNullException(nameof(fromAssetId));

            if (string.IsNullOrEmpty(toAssetId))
                throw new ArgumentNullException(nameof(toAssetId));

            if (fromAssetId == toAssetId)
                return new AmountAndPrice(fromAmount, 1, 1);

            var toAsset = await _assetsDict.GetItemAsync(toAssetId);

            var assetPair = (await _assetPairsDict.Values()).PairWithAssets(fromAssetId, toAssetId);

            var feedData = await _bestPriceRepository.GetAsync(assetPair.Id);

            var price = (assetPair.BaseAssetId == fromAssetId ? feedData.Ask : 1 / feedData.Ask).TruncateDecimalPlaces(assetPair.Accuracy);
            var invertedPrice = (assetPair.BaseAssetId == fromAssetId ? 1 / feedData.Ask : feedData.Ask).TruncateDecimalPlaces(assetPair.InvertedAccuracy);

            return new AmountAndPrice((price * fromAmount).TruncateDecimalPlaces(toAsset.Accuracy), price, invertedPrice);
        }

        public async Task<IEnumerable<IBalanceRecordWithBase>> FillBaseAssetData(IEnumerable<IBalanceRecord> balanceRecords,
            string toAssetId)
        {
            List<IBalanceRecordWithBase> result = new List<IBalanceRecordWithBase>();
            var marketProfile = await _bestPriceRepository.GetAsync();

            foreach (var record in balanceRecords)
            {
                result.Add(new BalanceRecordWithBase
                {
                    AssetId = record.AssetId,
                    Balance = record.Balance,
                    BaseAssetId = toAssetId,
                    AmountInBase = record.AssetId == toAssetId ? record.Balance :
                        await GetAmountInBase(record.AssetId, record.Balance, toAssetId, marketProfile)
                });
            }

            return result;
        }

        public async Task<IEnumerable<Tuple<string, double>>> GetAmountInBase(
            IEnumerable<Tuple<string, double>> balanceRecords, string toAssetId)
        {
            var result = new List<Tuple<string, double>>();

            var marketProfile = await _bestPriceRepository.GetAsync();

            foreach (var record in balanceRecords)
            {
                result.Add(new Tuple<string, double>(toAssetId,
                    await GetAmountInBase(record.Item1, record.Item2, toAssetId, marketProfile)));
            }

            return result;
        }

        public async Task<double> GetAmountInBase(string assetFrom, double amount, string assetTo,
            MarketProfile marketProfile = null)
        {
            var marketProfileData = marketProfile ?? await _bestPriceRepository.GetAsync();

            if (assetFrom == assetTo)
            {
                return amount;
            }

            if (Math.Abs(amount) < double.Epsilon)
                return 0;

            var assetPair = (await _assetPairsDict.Values()).PairWithAssets(assetFrom, assetTo);
            var askPrice = marketProfileData.Profile.First(x => x.Asset == assetPair.Id).Ask;
            var toAsset = await _assetsDict.GetItemAsync(assetTo);

            var price = assetPair.BaseAssetId == assetFrom ? askPrice : 1 / askPrice;

            return (price * amount).TruncateDecimalPlaces(toAsset.Accuracy);
        }

        public async Task<IEnumerable<ConversionResult>> GetMarketAmountInBase(IEnumerable<AssetWithAmount> from,
            string assetIdTo, OrderAction orderAction)
        {
            var limitOrdersTask = _activeLimitOrdersRepository.GetAsync();
            var assetsDictTask = _assetsDict.GetDictionaryAsync();
            var assetPairsTask = _assetPairsDict.Values();
            var marketProfileTask = _bestPriceRepository.GetAsync();

            var limitOrders = await limitOrdersTask;
            var assetsDict = await assetsDictTask;
            var assetPairs = await assetPairsTask;
            var marketProfile = await marketProfileTask;

            return @from.Select(item => GetMarketAmountInBase(orderAction, limitOrders, item, assetIdTo, assetsDict, assetPairs, marketProfile));
        }

        private ConversionResult GetMarketAmountInBase(OrderAction orderAction, IEnumerable<ILimitOrder> limitOrders, AssetWithAmount from,
            string assetTo, IDictionary<string, IAsset> assetsDict, IEnumerable<IAssetPair> assetPairs, MarketProfile marketProfile)
        {
            var result = new ConversionResult();
            var assetPair = assetPairs.PairWithAssets(from.AssetId, assetTo);

            if (!IsInputValid(from, assetTo, assetsDict) || assetPair == null)
            {
                result.SetResult(OperationResult.InvalidInputParameters);
                return result;
            }

            if (from.AssetId == assetTo)
            {
                result.From = result.To = from;
                result.Price = result.VolumePrice = 1;
                result.SetResult(OperationResult.Ok);
                return result;
            }

            limitOrders = limitOrders.Where(x => x.AssetPairId == assetPair.Id).GetAsync(orderAction, assetPair.IsInverted(assetTo));

            double sum = 0;
            double priceSum = 0;
            int n = 0;

            var neededSum = double.MaxValue;
            foreach (var order in limitOrders)
            {
                if (n != 0 && sum >= neededSum)
                    break;

                sum += Math.Abs(order.Volume);
                priceSum += order.Price;
                n++;
                neededSum = from.Amount * GetRate(assetTo, assetPair, priceSum / n);
            }

            if (n == 0)
            {
                result.SetResult(OperationResult.NoLiquidity);
                return result;
            }

            var price = priceSum / n;

            result.From = from;
            var rate = GetRate(assetTo, assetPair, price);
            var displayRate = GetRate(from.AssetId, assetPair, price);
            result.To = new AssetWithAmount
            {
                AssetId = assetTo,
                Amount = (rate * from.Amount).TruncateDecimalPlaces(assetsDict[assetTo].Accuracy, orderAction == OrderAction.Buy)
            };
            result.SetResult(sum < neededSum ? OperationResult.NoLiquidity : OperationResult.Ok);
            result.Price = GetRate(from.AssetId, assetPair, marketProfile.GetPrice(assetPair.Id, orderAction).GetValueOrDefault());
            result.VolumePrice = displayRate;

            return result;
        }

        private bool IsInputValid(AssetWithAmount @from, string assetTo, IDictionary<string, IAsset> assets)
        {
            if (from.Amount <= 0 || !assets.ContainsKey(assetTo) || !assets.ContainsKey(from.AssetId))
                return false;

            return true;
        }
    }
}