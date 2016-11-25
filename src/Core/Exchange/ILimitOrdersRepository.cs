using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Core.Assets;

namespace Core.Exchange
{
    public class MatchedOrder
    {
        public string Id { get; set; }
        public double Volume { get; set; }

        internal static MatchedOrder Create(IOrderBase orderBase, double volume)
        {
            return new MatchedOrder
            {
                Id = orderBase.Id,
                Volume = volume
            };
        }
    }

    public interface ILimitOrder : IOrderBase
    {
        double RemainingVolume { get; set; }
    }

    public class LimitOrder : ILimitOrder
    {
        public DateTime CreatedAt { get; set; }
        public DateTime MatchedAt { get; set; }
        public string Id { get; set; }
        public List<MatchedOrder> MatchedOrders { get; set; }
        public string ClientId { get; set; }
        public string BaseAsset { get; set; }
        public string AssetPairId { get; set; }
        public string Status { get; set; }
        public bool Straight { get; set; }
        public OrderAction OrderAction { get; set; }
        public string BlockChain { get; set; }
        public double Volume { get; set; }
        public double Price { get; set; }
        public double RemainingVolume { get; set; }

    }

    public interface IActiveLimitOrdersRepository
    {
        Task SaveAsync(ILimitOrder limitOrder, IAssetPair assetPair);

        Task<IEnumerable<ILimitOrder>> GetAsync(string assetPair);
        Task<ILimitOrder> GetByOrderIdAsync(string orderId);



        Task<IEnumerable<ILimitOrder>> GetNotTakenAsync(string assetPair, OrderAction orderAction);

        Task<bool> TakeOrderAsync(ILimitOrder limitOrder, IAssetPair assetPair);

        Task PutOrderBackAsync(ILimitOrder limitOrder, IAssetPair assetPair);

        Task CleanUpOrderAsync(ILimitOrder order, IAssetPair assetPair);
        Task<IEnumerable<ILimitOrder>> GetAsync();
        Task UpdateMatchedOrdersAndPutToMarketAsync(ILimitOrder limitOrder, IAssetPair assetPair);
    }

    public static class Ext
    {
        public static async Task<IEnumerable<ILimitOrder>> GetAsync(this IActiveLimitOrdersRepository repo, string assetPair, OrderAction action)
        {
            return (await repo.GetAsync(assetPair)).Where(x => action == OrderAction.Buy ? x.Volume > 0 : x.Volume < 0);
        }

        public static IEnumerable<ILimitOrder> GetAsync(this IEnumerable<ILimitOrder> limitOrders, OrderAction action, bool inverted)
        {
            action = inverted ? action.ViceVersa() : action;

            var orders = (limitOrders.Where(x => action == OrderAction.Buy ? x.Volume > 0 : x.Volume < 0)).ToArray();

            if (inverted)
            {
                foreach (var order in orders)
                {
                    order.Volume = order.Volume * order.Price * -1;
                    order.RemainingVolume = order.RemainingVolume * order.Price * -1;
                }
            }

            return action == OrderAction.Buy
                ? orders.OrderByDescending(x => x.Price)
                : orders.OrderBy(x => x.Price);
        }

        public static IEnumerable<ILimitOrder> GetSortedAsync(this IEnumerable<ILimitOrder> limitOrders, OrderAction action)
        {
            var orders = limitOrders.Where(x => action == OrderAction.Buy ? x.Volume > 0 : x.Volume < 0);
            return action == OrderAction.Buy
                ? orders.OrderByDescending(x => x.Price)
                : orders.OrderBy(x => x.Price);
        }
    }

}
