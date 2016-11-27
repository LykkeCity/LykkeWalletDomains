using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Core.Exchange;

namespace Core.Feed
{
    public interface IFeedData
    {
        string Asset { get; }
        DateTime DateTime { get; }
        double Bid { get; }
        double Ask { get; }
    }

    public class FeedData : IFeedData
    {
        public string Asset { get; set; }
        public DateTime DateTime { get; set; }
        public double Bid { get; set; }
        public double Ask { get; set; }

        public static FeedData Create(string asset, double bid, double ask, DateTime? dt = null)
        {
            return new FeedData
            {
                Asset = asset,
                Ask = ask,
                Bid = bid,
                DateTime = dt ?? DateTime.UtcNow
            };
        }
    }

    public class MarketProfile
    {
        public IEnumerable<IFeedData> Profile { get; set; }
    }

    public interface IAssetPairBestPriceRepository
    {
        Task<MarketProfile> GetAsync();
        Task<IFeedData> GetAsync(string assetPairId);

        Task SaveAsync(IFeedData feedData);

    }


    public static class FeedDataUtils
    {
        public static double GetPriceForRateConverter(this IFeedData feedData)
        {        //ToDo - Why we take ask here?
            return feedData.Ask;
        }

        public static double? GetAsk(this MarketProfile marketProfile, string assetPairId)
        {
            return marketProfile.Profile?.FirstOrDefault(x => x.Asset == assetPairId)?.Ask;
        }

        public static double? GetBid(this MarketProfile marketProfile, string assetPairId)
        {
            return marketProfile.Profile?.FirstOrDefault(x => x.Asset == assetPairId)?.Bid;
        }

        public static double? GetPrice(this MarketProfile marketProfile, string assetPairId, OrderAction orderAction)
        {
            return orderAction == OrderAction.Sell ? GetAsk(marketProfile, assetPairId) : GetBid(marketProfile, assetPairId);
        }
    }
}
